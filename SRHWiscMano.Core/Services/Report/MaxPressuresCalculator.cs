using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class MaxPressuresCalculator
    {
        public static IEnumerable<double> Calc(ITimeFrame tFrame)
        {
            // var intprSamples = tFrame.ExamData.InterpolatedSamples;
            //
            // var resultList = new List<List<double>>();
            // foreach (var region in tFrame.Regions.Items)
            // {
            //     var regionResult = region.SensorRange.AsEnumerable().Select(i =>
            //         intprSamples.SamplesInTimeRange(region.TimeRange).SmoothMax(i)).ToList();
            //     resultList.Add(regionResult);
            // }
            //
            // var reult2  = tFrame.Regions.Items
            //     .Select(region => region.SensorRange
            //         .AsEnumerable()
            //         .Select(i => intprSamples.SamplesInTimeRange(region.TimeRange).SmoothMax(i))
            //         .ToList())
            //     .SelectMany(m => m);

            var orgRes = RegionInterpolation.Sections()
                .Select(p => InterpolatedMaxesForRegion(tFrame, p.Item1, p.Item2)).SelectMany(m => m);

            return orgRes;
        }

        /// <summary>
        /// 입력된 Region 의 센서 영역에 대해서 각 센서의 SmoothMax 값을 계산하고 센서 값들 사이를 Interpolation 한 결과로 반환한다.
        /// </summary>
        /// <param name="tFrame"></param>
        /// <param name="regionType"></param>
        /// <param name="targetSize"></param>
        /// <returns></returns>
        private static IEnumerable<double> InterpolatedMaxesForRegion(
            ITimeFrame tFrame,
            RegionType regionType,
            int targetSize)
        {
            var resArr = tFrame.GetRegion(regionType).SensorRange.AsEnumerable().Select(i => new
                {
                    i,
                    tr = TimeRangeForRegionsOnSensor(tFrame, i)
                }).Where(_param1 => _param1.tr.HasValue)
                .Select(_param1 => tFrame.ExamData.Samples.SamplesInTimeRange(_param1.tr.Value).SmoothMax(_param1.i))
                .ToArray();

            return resArr;
            // return resArr.InterpolateTo(targetSize);
        }

        private static Interval? TimeRangeForRegionsOnSensor(ITimeFrame snapshot, int sensor)
        {
            IEnumerable<Interval> source = snapshot.Regions.Items.Where(r => r.SensorRange.Contains(sensor))
                .Select(r => r.TimeRange);
            return !source.Any() ? new Interval?() : source.Aggregate((p, c) => p.Union(c));
        }
    }
}