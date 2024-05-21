using MathNet.Numerics.Statistics;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class UesNadirCalculator
    {
        /// <summary>
        /// Snapshot의 region 에대한 센서 값을 계산하고, 각 sample 마다의 지정된 sensor 값들에 대한 mean 값을 계산한다.
        /// 이중에서 평균값이 제일 작은 sample을 찾는다.
        /// 또한, 연속된 sample 값에서의 가속도를 계산하고, 앞서 찾은 mean 값이 제일 작은 index를 기준으로 앞영역과 뒷영역에 대한 sample을 찾는다.
        /// 찾은 두영역에서의 최대 가속도 값을 갖는 sample을 찾고 이에대한 영역의 interval 을 찾는다.
        /// 즉, UES의 경우에는 Pre-UES 와 Post-UES의 사이에서는 압력이 거의 없기 때문에, mean 이 제일 작은 시점을 기준으로 Pre-UES, Post-UES
        /// 두개의 구간으로 나눌수 있다. 그리고 각 구간에서 압력의 가속도가 가장 높은 두지점을 찾고, 이 간격을 UES 구간이라고 계산하는 것이다.
        /// </summary>
        /// <param name="snapshot"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        /// 
        public static Duration CalcDuration(ITimeFrame timeFrame, IRegion region)
        {
            // snapshot 의 timerange와 sensor range에 대한 값에 대해서 Mean, std, variance등을 MathNet로 계산한 결과값을 만든다
            // 즉 Sample에서 Sensor의 전체 평균을 구한다.
            List<SampleStats> samples = TimeSampleExtensions.SamplesInTimeRange(timeFrame.ExamData.Samples, region.TimeRange)
                .Select(s => new SampleStats
                {
                    Sample = s,
                    Stats = new DescriptiveStatistics(s.ValuesForSensors(region.SensorRange))
                })
                .ToList();

            if (samples.Count < 3)
                return Duration.Zero;

            // Mean 값이 가작 작은 찾는다
            ElementIndex<SampleStats> sampleAndIndexOfMin = samples
                .Select((e, i) => new ElementIndex<SampleStats> {Element = e, Index = i})
                .MinBy(ai => ai.Element.Stats.Mean);

            // 평균값의 가속도를 계산한다
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