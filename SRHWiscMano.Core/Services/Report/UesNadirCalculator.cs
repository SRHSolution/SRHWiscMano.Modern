using MathNet.Numerics.Statistics;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class UesNadirCalculator
    {
        public static Duration CalcDuration(ITimeFrame timeFrame, IRegion region)
        {
            List<SampleStats> samples = TimeSampleExtensions.SamplesInTimeRange(timeFrame.ExamData.Samples, region.TimeRange)
                .Select(s => new SampleStats
                {
                    Sample = s,
                    Stats = new DescriptiveStatistics(s.ValuesForSensors(region.SensorRange))
                })
                .ToList();

            if (samples.Count < 3)
                return Duration.Zero;

            ElementIndex<SampleStats> sampleAndIndexOfMin = samples
                .Select((e, i) => new ElementIndex<SampleStats> {Element = e, Index = i})
                .MinBy(ai => ai.Element.Stats.Mean);

            List<double> meanDifferences = samples.Select(t => t.Stats.Mean).Differences().Differences().ToList();
            int index1 = 0, index2 = 0;

            if (meanDifferences.Count > 1)
            {
                var source1 = meanDifferences.Select((e, i) => new ElementIndex<double> {Element = e, Index = i})
                    .Where(di => di.Index <= sampleAndIndexOfMin.Index);
                var source2 = meanDifferences.Select((e, i) => new ElementIndex<double> {Element = e, Index = i})
                    .Where(di => di.Index > sampleAndIndexOfMin.Index);

                if (source1.Any() && source2.Any())
                {
                    index1 = source1.MaxBy(di => di.Element).Index + 1;
                    index2 = source2.MaxBy(di => di.Element).Index + 1;
                }
            }

            var data = samples[index1];
            return samples[index2].Sample.Time - data.Sample.Time;
        }

        public static double CalcMinimum(ITimeFrame snapshot, IRegion region)
        {
            Range<int> range = region.SensorRange.Span() < 2
                ? region.SensorRange
                : Range.Create(region.SensorRange.Start, region.SensorRange.Start + 1);

            var source = range.AsEnumerable()
                .Select(si => new SensorSample
                {
                    Sensor = si,
                    Sample = snapshot.ExamData.Samples.SamplesInTimeRange(region.TimeRange).MinBy(s => s.Values[si])
                });

            Duration duration = Duration.FromMilliseconds(250L);

            return source
                .Select(ss => new {ss, timeRange = ss.Sample.Time.AtCenterOfDuration(duration, region.TimeRange)})
                .Select(x => new
                {
                    x,
                    stats = new DescriptiveStatistics(snapshot.ExamData.Samples.SamplesInTimeRange(x.timeRange)
                        .Select(s => s.Values[x.ss.Sensor]))
                })
                .Select(x => x.stats.Mean)
                .Min();
        }

        public class SampleStats
        {
            public TimeSample Sample { get; set; }
            public DescriptiveStatistics Stats { get; set; }
        }

        public class ElementIndex<T>
        {
            public T Element { get; set; }
            public int Index { get; set; }
        }

        public class SensorSample
        {
            public int Sensor { get; set; }
            public TimeSample Sample { get; set; }
        }
    }
}