﻿using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using System.Windows.Markup;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Services.Detection
{
    public class RegionFinder : IRegionFinder
    {
        private static readonly Duration InitialWidthVP = Duration.FromMilliseconds(2000L);
        private static readonly Duration InitialWidthMP = Duration.FromMilliseconds(1600L);

        public Region Find(
            RegionType type,
            ITimeFrame state,
            DataPoint click,
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
                default:
                    throw new ArgumentException("Invalid region type", nameof(type));
            }
        }

        private static Region FindVP(
            ITimeFrame state,
            DataPoint click,
            RegionFinderConfig config,
            IDetectionDiagnostics diagnostics)
        {
            // Region region = state.VPUpperBound.HasValue
            //     ? CreateRegionWithSensorsFromVPLine(state, click, state.VPUpperBound.Value, 2, RegionType.VP)
            //     : CreateRegionWithRelativeSensors(state, click, 1, 1, RegionType.VP);
            Region region = CreateRegionWithRelativeSensors(state, click, 1, 1, RegionType.VP);
            RegionFinderRegionConfig configForRegion = config.GetConfigForRegion(RegionType.VP);
            Duration initialWidth = configForRegion.InitialWidth;
            return region.SetTimeCenteredOnClickPoint(initialWidth).AdjustToPointOfMaximumInCenter().DetermineVPBounds(
                configForRegion.Algorithm,new DiagnosticsContext(diagnostics, RegionType.VP));
        }

        private static Region FindPreUES(ITimeFrame state, DataPoint click)
        {
            return null;

            // return CreateRegionWithSensorsDownToUesLine(state, click, state.UesLowerBound.Value, RegionType.PreUES)
            //     .SetTimeCenteredOnClickPoint(Duration.FromMilliseconds(500L)).AdjustToPointOfMaximumOnRightEdge()
            //     .AdjustToPeakChangeRight(Duration.FromMilliseconds(500L))
            //     .ExtendTimeLeft(Duration.FromMilliseconds(1000L));
        }

        private static Region FindPostUES(ITimeFrame state, DataPoint click)
        {
            return null;
            // if (!state.UesLowerBound.HasValue)
            //     throw new RegionFinderException("UES lower bound must be set");
            // return CreateRegionWithSensorsDownToUesLine(state, click, state.UesLowerBound.Value, RegionType.PostUES)
            //     .SetTimeCenteredOnClickPoint(Duration.FromMilliseconds(1000L)).AdjustToPointOfMaximumOnLeftEdge()
            //     .AdjustToPeakChangeLeft(Duration.FromMilliseconds(500L));
        }

        private static Region FindUES(ITimeFrame state, DataPoint click)
        {
            IRegion regionOrThrow1 =
                FindRegionOrThrow(state, RegionType.PreUES, "PreUES must be determined before UES");
            IRegion regionOrThrow2 =
                FindRegionOrThrow(state, RegionType.PostUES, "PostUES must be determined before UES");
            if (regionOrThrow1.FocalPoint.Time >= regionOrThrow2.FocalPoint.Time)
                throw new RegionFinderException("The PreUES region should be earlier than the PostUES region");
            int sensorTop = Math.Min(regionOrThrow1.SensorRange.Start, regionOrThrow2.SensorRange.Start);
            int sensorBottom = Math.Max(regionOrThrow1.SensorRange.End, regionOrThrow2.SensorRange.End);
            Interval timeRange = new Interval(regionOrThrow1.FocalPoint.Time, regionOrThrow2.FocalPoint.Time);
            return CreateRegionWithSensors(state, click, sensorTop, sensorBottom, RegionType.UES).ChangeTime(timeRange);
        }

        private static Region FindTB(
            ITimeFrame state,
            DataPoint click,
            RegionFinderConfig config,
            IDetectionDiagnostics diagnostics)
        {
            IRegion regionOrThrow1 = FindRegionOrThrow(state, RegionType.VP, "VP must be determined before TB");
            IRegion regionOrThrow2 =
                FindRegionOrThrow(state, RegionType.PostUES, "PostUES must be determined before TB");
            int end = regionOrThrow1.SensorRange.End;
            int start = regionOrThrow2.SensorRange.Start;
            int num1 = end < start
                ? Range.Create(end, start).Span()
                : throw new RegionFinderException("VP must be above PostUES");
            if (num1 < 2)
                throw new RegionFinderException("There must be at least 2 sensors between the VP and UES");
            int num2 = num1 - num1 / 2;
            RegionFinderRegionConfig configForRegion = config.GetConfigForRegion(RegionType.TB);

            return null;

            // return CreateRegionWithSensors(state, click, end, end + num2, RegionType.TB)
            //     .SetTimeCenteredOnPoint(regionOrThrow1.FocalPoint.Time, configForRegion.InitialWidth)
            //     .AdjustToTimeRangeAtEdges(configForRegion.Algorithm,
            //         new DiagnosticsContext(diagnostics, RegionType.TB));
        }

        private static Region FindHP(
            ITimeFrame state,
            DataPoint click,
            RegionFinderConfig config,
            IDetectionDiagnostics diagnostics)
        {
            FindRegionOrThrow(state, RegionType.VP, "VP must be determined before HP");
            IRegion regionOrThrow1 =
                FindRegionOrThrow(state, RegionType.PostUES, "PostUES must be determined before HP");
            IRegion regionOrThrow2 = FindRegionOrThrow(state, RegionType.TB, "TB must be determined before HP");
            int end = regionOrThrow2.SensorRange.End;
            int start = regionOrThrow1.SensorRange.Start;
            if (end >= start)
                throw new RegionFinderException("TB must be above PostUES");
            RegionFinderRegionConfig configForRegion = config.GetConfigForRegion(RegionType.HP);

            return null;
            // return CreateRegionWithSensors(state, click, end, start, RegionType.HP)
                // .SetTimeStartingAtPoint(regionOrThrow2.TimeRange.Start, configForRegion.InitialWidth)
                // .AdjustToTimeRangeAtEdges(configForRegion.Algorithm,
                    // new DiagnosticsContext(diagnostics, RegionType.HP));
        }

        private static Region FindMP(ITimeFrame state, DataPoint click)
        {
            IRegion regionOrThrow1 = FindRegionOrThrow(state, RegionType.VP, "VP must be determined before MP");
            IRegion regionOrThrow2 =
                FindRegionOrThrow(state, RegionType.PostUES, "PostUES must be determined before MP");
            int end = regionOrThrow1.SensorRange.End;
            int start = regionOrThrow2.SensorRange.Start;
            if (end >= start)
                throw new RegionFinderException("VP must be above PostUES");

            return null;
            // return CreateRegionWithSensors(state, click, end, start, RegionType.MP)
                // .SetTimeCenteredOnPoint(regionOrThrow1.FocalPoint.Time, InitialWidthMP)
                // .AdjustToTimeRangeAboveBaseline(InitialWidthMP, new DiagnosticsContext(null, RegionType.MP));
        }

        private static Region CreateRegionWithRelativeSensors(
            ITimeFrame window,
            DataPoint click,
            int sensorsAbove,
            int sensorsBelow,
            RegionType type)
        {
            
            int sensorTop = Math.Max(click.Sensor - sensorsAbove, window.SensorRange().Lesser);
            int sensorBottom = Math.Min(click.Sensor + sensorsBelow + 1, window.SensorRange().Greater);
            return CreateRegionWithSensors(window, click, sensorTop, sensorBottom, type);
        }

        private static Region CreateRegionWithSensorsFromVPLine(
            ITimeFrame window,
            DataPoint click,
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
            DataPoint click,
            RegionType type)
        {
            // return CreateRegionWithRelativeSensors(window, click, 0, uesLowerBound - click.Sensor, type);
            return CreateRegionWithRelativeSensors(window, click, 0, click.Sensor, type);
        }

        private static Region CreateRegionWithSensors(
            ITimeFrame window,
            DataPoint click,
            int sensorTop,
            int sensorBottom,
            RegionType type)
        {
            return new Region(window, Range.Create(sensorTop, sensorBottom), type, click, null);
        }

        private static IRegion FindRegionOrThrow(
            ITimeFrame snapshot,
            RegionType regionType,
            string exceptionMessage)
        {
            return snapshot.Regions.FirstOrDefault(r => r.Type == regionType) ??
                   throw new RegionFinderException(exceptionMessage);
        }

        
    }
}