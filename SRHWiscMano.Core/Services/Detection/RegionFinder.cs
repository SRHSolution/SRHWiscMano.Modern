using System.Diagnostics;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using System.Windows.Markup;
using Accessibility;
using DynamicData;
using NLog;
using Range = SRHWiscMano.Core.Helpers.Range;
using Microsoft.Extensions.Logging;
using SRHWiscMano.Core.Services.Report;
using SRHWiscMano.Core.ViewModels;
using SkiaSharp;

namespace SRHWiscMano.Core.Services.Detection
{
    public class RegionFinder : IRegionFinder
    {
        private static readonly Duration InitialWidthVP = Duration.FromMilliseconds(2000L);
        private static readonly Duration InitialWidthMP = Duration.FromMilliseconds(1600L);

        public Region Find(
            RegionType type,
            ITimeFrame state,
            SamplePoint click,
            RegionFinderConfig config,
            IDetectionDiagnostics diagnostics)
        {
            if (click != null && !state.SensorRange().Contains(click.Sensor))
                throw new RegionFinderException("Clicked sensor must be within the sensor range");
            switch (type)
            {
                case RegionType.VP:
                    return FindVP(state, click, config, diagnostics);
                case RegionType.PreUES:
                    return FindPreUES(state, click);
                case RegionType.PostUES:
                    return FindPostUES(state, click);
                case RegionType.MP:
                    return FindMP(state, click);
                case RegionType.TB:
                    return FindTB(state, click, config, diagnostics);
                case RegionType.HP:
                    return FindHP(state, click, config, diagnostics);
                case RegionType.UES:
                    return FindUES(state, click);
                case RegionType.UESNdir:
                    return FindUESNadir(state, click);
                default:
                    throw new ArgumentException("Invalid region type", nameof(type));
            }
        }

        /// <summary>
        /// 클릭한 위치를 기준으로 찾는다
        /// </summary>
        /// <param name="state"></param>
        /// <param name="click"></param>
        /// <param name="config"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        private static Region FindVP(
            ITimeFrame state,
            SamplePoint click,
            RegionFinderConfig config,
            IDetectionDiagnostics diagnostics)
        {
            Region region = CreateRegionWithRelativeSensors(state, click, 1, 1, RegionType.VP);
            RegionFinderRegionConfig configForRegion = config.GetConfigForRegion(RegionType.VP);
            Duration initialWidth = configForRegion.InitialWidth;


            var start1 = region.TimeRange.Start.ToUnixTimeMilliseconds();
            var end1 = region.TimeRange.End.ToUnixTimeMilliseconds();
            Debug.WriteLine($"Region {start1}~{end1}");
            var updRegion = region.SetTimeCenteredOnClickPoint(initialWidth);
            var start2 = updRegion.TimeRange.Start.ToUnixTimeMilliseconds();
            var end2 = updRegion.TimeRange.End.ToUnixTimeMilliseconds();
            Debug.WriteLine($"Region {start2}~{end2}");
            return region.SetTimeCenteredOnClickPoint(initialWidth).AdjustToPointOfMaximumInCenter().DetermineVPBounds(configForRegion.Algorithm,new DiagnosticsContext(diagnostics, RegionType.VP));
        }

        private static Region FindPreUES(ITimeFrame state, SamplePoint click)
        {
            return CreateRegionWithSensorsDownToUesLine(state, click, RegionType.PreUES)
            .SetTimeCenteredOnClickPoint(Duration.FromMilliseconds(500L)).AdjustToPointOfMaximumOnRightEdge()
            .AdjustToPeakChangeRight(Duration.FromMilliseconds(500L))
            .ExtendTimeLeft(Duration.FromMilliseconds(1000L));
        }

        private static Region FindPostUES(ITimeFrame state, SamplePoint click)
        {
            return CreateRegionWithSensorsDownToUesLine(state, click, RegionType.PostUES)
            .SetTimeCenteredOnClickPoint(Duration.FromMilliseconds(1000L)).AdjustToPointOfMaximumOnLeftEdge()
            .AdjustToPeakChangeLeft(Duration.FromMilliseconds(500L));
        }

        private static Region FindUES(ITimeFrame state, SamplePoint click)
        {
            IRegion regionPreUES =
                FindRegionOrThrow(state, RegionType.PreUES, "PreUES must be determined before UES");
            IRegion regionPostUES =
                FindRegionOrThrow(state, RegionType.PostUES, "PostUES must be determined before UES");
            if (regionPreUES.FocalPoint.Time >= regionPostUES.FocalPoint.Time)
                throw new RegionFinderException("The PreUES region should be earlier than the PostUES region");

            // UES는 PreUES와 PostUES를 모두포함하는 최대 영역으로 잡는다
            int sensorTop = Math.Min(regionPreUES.SensorRange.Start, regionPostUES.SensorRange.Start);
            int sensorBottom = Math.Max(regionPreUES.SensorRange.End, regionPostUES.SensorRange.End);
            Interval timeRange = new Interval(regionPreUES.FocalPoint.Time, regionPostUES.FocalPoint.Time);
            return CreateRegionWithSensors(state, click, sensorTop, sensorBottom, RegionType.UES).ChangeTime(timeRange);
        }

        private static Region FindUESNadir(ITimeFrame state, SamplePoint click)
        {

            IRegion regionUES = FindRegionOrThrow(state, RegionType.UES, "UES must be determined before UES");

            var minUes = UesNadirCalculator.CalcMinimum2(state, regionUES);
            var nadirDuration = UesNadirCalculator.CalcDuration(state, regionUES);

            var frameStartIdx = state.ExamData.SampleIndexFromTime(state.TimeRange().Start);
            var minUesIndex = state.ExamData.SampleIndexFromTime(minUes.sensorSample.Sample.Time);
            var offsetIdx = minUesIndex - frameStartIdx;
            Duration duration = Duration.FromMilliseconds(250L);
            var minUesTimeRange = minUes.sensorSample.Sample.Time.AtCenterOfDuration(duration, regionUES.TimeRange);

            var minUesSensor = minUes.sensorSample.Sensor;
            // UES는 PreUES와 PostUES를 모두포함하는 최대 영역으로 잡는다
            return CreateRegionWithSensors(state, click, minUesSensor, minUesSensor+1, RegionType.UESNdir).ChangeTime(minUesTimeRange);
        }


        /// <summary>
        /// 센서의 크기는 VP - PostUES 간격의 센서 크기에서 상위 절반은 TB, 하위 절반은 HP로 한다. 갯수가 홀수일 경우엔 TB가 한개더 적다
        /// </summary>
        /// <param name="state"></param>
        /// <param name="click"></param>
        /// <param name="config"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        /// <exception cref="RegionFinderException"></exception>
        private static Region FindTB(
            ITimeFrame state,
            SamplePoint click,
            RegionFinderConfig config,
            IDetectionDiagnostics diagnostics)
        {
            IRegion regionVP = FindRegionOrThrow(state, RegionType.VP, "VP must be determined before TB");
            IRegion regionPostUES =
                FindRegionOrThrow(state, RegionType.PostUES, "PostUES must be determined before TB");
            int gapTop = regionVP.SensorRange.End;
            int gapBottom = regionPostUES.SensorRange.Start;
            
            // VP ~ PostUES 사이의 Sensor의 크기 값 + 1을 구한다, 이는 TB가 HP 보다 최소 같거나 더 절반이상의 크기를 갖게 하기 위함이다.
            int num1 = gapTop < gapBottom
                ? gapBottom - gapTop + 1
                : throw new RegionFinderException("VP must be above PostUES");

            if (num1 < 2)
                throw new RegionFinderException("There must be at least 2 sensors between the VP and UES");
            var num2 = num1 - num1/2;
            RegionFinderRegionConfig configForRegion = config.GetConfigForRegion(RegionType.TB);

            return CreateRegionWithSensors(state, click, gapTop, gapTop + num2, RegionType.TB)
            .SetTimeCenteredOnPoint(regionVP.FocalPoint.Time, configForRegion.InitialWidth)
            .AdjustToTimeRangeAtEdges(configForRegion.Algorithm,
            new DiagnosticsContext(diagnostics, RegionType.TB));
        }

        /// <summary>
        /// 센서의 크기는 VP - PostUES 간격의 센서 크기에서 상위 절반은 TB, 하위 절반은 HP로 한다. 갯수가 홀수일 경우엔 TB가 한개더 적다
        /// </summary>
        /// <param name="state"></param>
        /// <param name="click"></param>
        /// <param name="config"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        /// <exception cref="RegionFinderException"></exception>
        private static Region FindHP(
            ITimeFrame state,
            SamplePoint click,
            RegionFinderConfig config,
            IDetectionDiagnostics diagnostics)
        {
            FindRegionOrThrow(state, RegionType.VP, "VP must be determined before HP");
            IRegion regionPostUES =
                FindRegionOrThrow(state, RegionType.PostUES, "PostUES must be determined before HP");
            IRegion regionTB = FindRegionOrThrow(state, RegionType.TB, "TB must be determined before HP");
            int tbEnd = regionTB.SensorRange.End;
            int uesStart = regionPostUES.SensorRange.Start;
            if (tbEnd > uesStart)
                throw new RegionFinderException("TB must be above PostUES");

            RegionFinderRegionConfig configForRegion = config.GetConfigForRegion(RegionType.HP);
           
            return CreateRegionWithSensors(state, click, tbEnd, uesStart, RegionType.HP)
                .SetTimeStartingAtPoint(regionTB.TimeRange.Start, configForRegion.InitialWidth)
                .AdjustToTimeRangeAtEdges(configForRegion.Algorithm,
                    new DiagnosticsContext(diagnostics, RegionType.HP));
        }

        private static Region FindMP(ITimeFrame state, SamplePoint click)
        {
            IRegion regionOrThrow1 = FindRegionOrThrow(state, RegionType.VP, "VP must be determined before MP");
            IRegion regionOrThrow2 =
                FindRegionOrThrow(state, RegionType.PostUES, "PostUES must be determined before MP");
            int end = regionOrThrow1.SensorRange.End;
            int start = regionOrThrow2.SensorRange.Start;
            if (end >= start)
                throw new RegionFinderException("VP must be above PostUES");

            return CreateRegionWithSensors(state, click, end, start, RegionType.MP)
                .SetTimeCenteredOnPoint(regionOrThrow1.FocalPoint.Time, InitialWidthMP)
                .AdjustToTimeRangeAboveBaseline(InitialWidthMP, new DiagnosticsContext(null, RegionType.MP));
        }


        /// <summary>
        /// 클릭한 Point를 기준으로 above, below 크기만큼의 Region 센서 영역을 지정한다.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="click"></param>
        /// <param name="sensorsAbove"></param>
        /// <param name="sensorsBelow"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Region CreateRegionWithRelativeSensors(
            ITimeFrame window,
            SamplePoint click,
            int sensorsAbove,
            int sensorsBelow,
            RegionType type)
        {
            int sensorTop = Math.Max(click.Sensor - sensorsAbove, window.SensorRange().Lesser);
            // Range의 end는 포함하지 않는 크기이므로 +1 더해서 지정한다
            int sensorBottom = Math.Min(click.Sensor + sensorsBelow+1, window.SensorRange().Greater);
            return CreateRegionWithSensors(window, click, sensorTop, sensorBottom, type);

        }

        private static Region CreateRegionWithSensorsFromVPLine(
            ITimeFrame window,
            SamplePoint click,
            int vpUpperBound,
            int sensorsBelow,
            RegionType type)
        {
            int sensorTop = Math.Max(vpUpperBound, window.SensorRange().Lesser);
            int sensorBottom = Math.Min(vpUpperBound + sensorsBelow + 1, window.SensorRange().Greater);
            return CreateRegionWithSensors(window, click, sensorTop, sensorBottom, type);
        }

        private static Region CreateRegionWithSensorsDownToUesLine(
            ITimeFrame window,
            SamplePoint click,
            RegionType type)
        {
            return CreateRegionWithRelativeSensors(window, click, 0, (int)window.MaxSensorBound - click.Sensor, type);
        }

        private static Region CreateRegionWithSensors(
            ITimeFrame window,
            SamplePoint click,
            int sensorTop,
            int sensorBottom,
            RegionType type)
        {
            return new Region(window, InstantUtils.EmptyInterval,Range.Create(sensorTop, sensorBottom), type, click, null);
        }

        private static IRegion FindRegionOrThrow(
            ITimeFrame window,
            RegionType regionType,
            string exceptionMessage)
        {
            return window.Regions.Items.FirstOrDefault(r => r.Type == regionType) ??
                   throw new RegionFinderException(exceptionMessage);
        }

        
    }
}