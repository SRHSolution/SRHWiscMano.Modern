using MathNet.Numerics.Statistics;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using System.Windows.Markup;
using MoreLinq;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Services.Report
{
    public static class UesNadirCalculator
    {
        /// <summary>
        /// Snapshot의 region 에대한 센서 값을 계산하고, 각 sample 마다의 지정된 sensor 값들에 대한 mean 값을 계산한다.
        /// 이중에서 평균값이 제일 작은 sample을 찾는다.
        /// 또한, 연속된 sample 값에서의 가속도를 계산하고, 앞서 찾은 mean 값이 제일 작은 index를 기준으로 앞영역과 뒷영역에 대한 sample을 찾는다.
        /// 찾은 두영역에서의 최대 가속도 값을 갖는 sample을 찾고 이에대한 영역의 interval 을 찾는다.
        /// 즉, UES의 경우에는 Pre-UES 와 Post-UES의 사이에서는 압력이 거의 없기 때문에, mean 이 제일 작은 시점을 기준으로 Pre-UES, Post-UES
        /// 두개의 구간으로 나눌수 있다. 그리고 각 구간에서 압력의 가속도가 가장 높은 두지점을 찾고, 이 간격을 UES 구간이라고 계산하는 것이다.
        /// </summary>
        /// <param name="tFrame"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        /// 
        public static Duration CalcDuration(ITimeFrame tFrame, IRegion region)
        {
            // tFrame 의 timerange와 sensor range에 대한 값에 대해서 Mean, std, variance등을 MathNet로 계산한 결과값을 만든다
            // 즉 Sample에서 Sensor의 전체 평균을 구한다.
            List<SampleStats> samples = TimeSampleExtensions
                .SamplesInTimeRange(tFrame.ExamData.Samples, region.TimeRange)
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
                .Select((e, i) => new ElementIndex<SampleStats> { Element = e, Index = i })
                .MinBy(ai => ai.Element.Stats.Mean);

            // 평균값의 가속도를 계산한다
            List<double> meanDifferences = samples.Select(t => t.Stats.Mean).Differences().Differences().ToList();
            int peakIdxLeft = 0, peakIdxRight = 0;

            if (meanDifferences.Count > 1)
            {
                var source1 = meanDifferences.Select((e, i) => new ElementIndex<double> { Element = e, Index = i })
                    .Where(di => di.Index <= sampleAndIndexOfMin.Index);
                var source2 = meanDifferences.Select((e, i) => new ElementIndex<double> { Element = e, Index = i })
                    .Where(di => di.Index > sampleAndIndexOfMin.Index);

                if (source1.Any() && source2.Any())
                {
                    peakIdxLeft = source1.MaxBy(di => di.Element).Index + 1;
                    peakIdxRight = source2.MaxBy(di => di.Element).Index + 1;
                }
            }

            var data = samples[peakIdxLeft];
            return samples[peakIdxRight].Sample.Time - data.Sample.Time;
        }

        public static (SensorSample sensorSample, double Mean) CalcMinimum(ITimeFrame tFrame, IRegion region)
        {
            Range<int> range = region.SensorRange.Span() < 2
                ? region.SensorRange
                : Range.Create(region.SensorRange.Start, region.SensorRange.Start + 1);

            // 센서 range에서 각 센서별 가장 찾은 센서 값을 찾는다
            var source = range.AsEnumerable()
                .Select(si => new SensorSample
                {
                    Sensor = si,
                    Sample = tFrame.ExamData.Samples.SamplesInTimeRange(region.TimeRange).MinBy(s => s.Values[si])
                });


            Duration duration = Duration.FromMilliseconds(250L);

            var minSource = source
                // 최소값을 갖고 있는 샘플의 시간을 중심으로 0.25초 앞뒤의 duration 을 갖도록 range를 설정한다.
                .Select(ss => new { ss, timeRange = ss.Sample.Time.AtCenterOfDuration(duration, region.TimeRange) })
                //
                .Select(x => new
                {
                    x,
                    // 0.25초의 중심 영역에서의 모든 센서값을 분석한다
                    stats = new DescriptiveStatistics(tFrame.ExamData.Samples.SamplesInTimeRange(x.timeRange)
                        .Select(s => s.Values[x.ss.Sensor]))
                })
                .MinBy(x => x.stats.Mean);

            return (minSource.x.ss, minSource.stats.Mean);
            // .Select(x => x.stats.Mean)
            // .Min();
        }

        public static (SensorSample sensorSample, double Mean) CalcMinimum2(ITimeFrame tFrame, IRegion region)
        {
            // 센서 range에서의 최대 값데이터를 만든다
            var maxValues = region.Window.ExamData
                .MaxValueForSensorInTimeRange(region.Window.TimeRange(), region.SensorRange).ToList();

            var samples = tFrame.ExamData.Samples.SamplesInTimeRange(region.TimeRange);
            // UES 센서 영역내의 각 sample 에서 가장 큰 값을 갖는 데이터를 선택된 sensor의 index와 함께 Enumerable로 저장한다
            var maxSource= samples.Select((si, id) => new ElementIndex<SensorSample>()
            {
                Index = id,
                Element = new SensorSample()
                {
                    Sensor = si.ValuesForSensors(region.SensorRange).Select((vv, ii) =>
                        new { Value = vv, Index = ii }).OrderByDescending(x => x.Value).First().Index + region.SensorRange.Start,
                    Sample = si
                }
            });
            // 센서 range에서 각 센서별 가장 찾은 센서 값을 찾는다

            Duration duration = Duration.FromMilliseconds(250L);

            var minSource = maxSource.Window(3).Select((ss, ii) =>
                    new {Element = ss, Avg = ss.Average(ei => ei.Element.Sample.Values[ei.Element.Sensor]), Index = ii })
                .MinBy(mm => mm.Avg);


            return (minSource.Element[0].Element, minSource.Avg);
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

        /// <summary>
        /// Sample 과 분석에 적용된 Sensor 값을 갖고 있음
        /// </summary>
        public class SensorSample
        {
            public int Sensor { get; set; }
            public TimeSample Sample { get; set; }
        }
    }
}