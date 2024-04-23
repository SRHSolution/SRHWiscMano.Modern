using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class PressureAtTBMaxCalculator
    {
        public static IEnumerable<double> Calc(ITimeFrame timeFrame)
        {
            RegionType type = RegionType.TB;
            IRegion mp = timeFrame.GetRegion(type);
            TimeSample sample =
                (mp.SensorRange.Span() > 2
                    ? Range.Create(mp.SensorRange.Start, mp.SensorRange.Start + 2)
                    : mp.SensorRange).AsEnumerable()
                .Select(i => TimeSampleExtensions.SampleValuesForSensorInTimeRange(timeFrame.ExamData,mp.TimeRange, i)).SelectMany(t => t)
                .MaxBy(t => t.Item2).Item1;
            return timeFrame.InterpolatedValuesAtSample(sample);
        }
    }
}