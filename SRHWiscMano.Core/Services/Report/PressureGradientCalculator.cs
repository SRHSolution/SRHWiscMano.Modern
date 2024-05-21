using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class PressureGradientCalculator
    {
        public static IEnumerable<double> CalcDefaultRange(ITimeFrame state)
        {
            return Calc(state, Duration.FromMilliseconds(1000L), Duration.FromMilliseconds(5000L));
        }

        public static IEnumerable<double> Calc(
            ITimeFrame state,
            Duration timeBeforeVPStart,
            Duration totalTime)
        {
            IRegion region1 = state.GetRegion(RegionType.VP);
            IRegion region2 = state.GetRegion(RegionType.UES);
            Range<int> topSensorRange = Range.Create<int>(region1.SensorRange.Start, region2.SensorRange.Start);
            Range<int> bottomSensorRange = region2.SensorRange;
            Instant start = region1.TimeRange.Start - timeBeforeVPStart;
            Interval timeRange = new Interval(start, start + totalTime);
            return state.ExamData.Samples.SamplesInTimeRange(timeRange)
                .Select(s => CalcGradient(s, topSensorRange, bottomSensorRange));
        }

        private static double CalcGradient(
            TimeSample sample,
            Range<int> sensorsAbove,
            Range<int> sensorsBelow)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            for (int index = 0; index < sample.Values.Count; ++index)
                if (sensorsAbove.Contains(index))
                    num1 += sample.Values[index];
                else if (sensorsBelow.Contains(index))
                    num2 += sample.Values[index];
            return num1 - num2;
        }
    }
}