using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class MaxPressuresCalculator
    {
        public static IEnumerable<double> Calc(ITimeFrame snapshot)
        {
            return RegionInterpolation.Sections()
                .Select(p => InterpolatedMaxesForRegion(snapshot, p.Item1, p.Item2)).SelectMany(m => m);
        }

        private static IEnumerable<double> InterpolatedMaxesForRegion(
            ITimeFrame snapshot,
            RegionType regionType,
            int targetSize)
        {
            return snapshot.GetRegion(regionType).SensorRange.AsEnumerable().Select(i => new
                {
                    i,
                    tr = TimeRangeForRegionsOnSensor(snapshot, i)
                }).Where(_param1 => _param1.tr.HasValue)
                .Select(_param1 => snapshot.ExamData.Samples.SamplesInTimeRange(_param1.tr.Value).SmoothMax(_param1.i)).ToArray()
                .InterpolateTo(targetSize);
        }

        private static Interval? TimeRangeForRegionsOnSensor(ITimeFrame snapshot, int sensor)
        {
            IEnumerable<Interval> source = snapshot.Regions.Items.Where(r => r.SensorRange.Contains(sensor))
                .Select(r => r.TimeRange);
            return !source.Any() ? new Interval?() : source.Aggregate((p, c) => p.Union(c));
        }
    }
}