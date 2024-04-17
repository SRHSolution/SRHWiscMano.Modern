using MathNet.Numerics.Statistics;
using NLog;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services.Detection
{
    /// <summary>
    /// 처음에 잡은 range 의 주변부에 있는 시간영역의 데이터를 기반으로 baseline(레퍼런스 threshold)를 만들고, 이를 넘어가는 trigger 값을 detect 해서 region을 새롭게 잡는 알고리즘이다
    /// </summary>
    internal class RangeAboveBaseline : IEdgeAlgorithm
    {
        private const double UpperLimitMean = 25.0;
        private const double UpperLimitStdDev = 5.0;
        private const double AutoDetectStdDevMult = 2.0;
        private const double AutoDetectStdDevOffset = 10.0;
        private const double RiseSlopeThreshold = 0.4;
        private static readonly Duration RiseSlopeWindowDuration = Duration.FromMilliseconds(100L);
        private static readonly Duration FalseTroughDetectionDuration = Duration.FromMilliseconds(40L);
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IReadOnlyCollection<Interval> backgroundWindows;
        private Duration backgroundWindowWidth;
        private Duration inset;

        public RangeAboveBaseline(Duration backgroundWindowWidth, Duration inset)
        {
            this.backgroundWindowWidth = backgroundWindowWidth;
            this.inset = inset;
            backgroundWindows = null;
        }

        public RangeAboveBaseline(IReadOnlyCollection<Interval> backgroundWindows)
        {
            backgroundWindowWidth = new Duration();
            inset = new Duration();
            this.backgroundWindows = backgroundWindows;
        }

        public Interval? FindOnsetEdges(
            IExamination data,
            int sensorIndex,
            Interval timeRange,
            DiagnosticsContext diag)
        {
            if (backgroundWindows == null)
                backgroundWindows = BackgroundWindows.Default(timeRange, backgroundWindowWidth, inset, data.TotalTime()).ToList();
            Track(diag, sensorIndex, timeRange);
            Interval? onsetEdges = DoFindOnsetEdges(data, sensorIndex, timeRange, backgroundWindows, diag);
            if (onsetEdges.HasValue)
                diag.TrackEdges(onsetEdges.Value, sensorIndex, DetectionMarkerType.Edge);
            return onsetEdges;
        }

        public static dynamic CalculateBaselineStats(
            IExamination data,
            int sensorIndex,
            IEnumerable<Interval> backgroundWindows)
        {
            DescriptiveStatistics descriptiveStatistics =
                new DescriptiveStatistics(
                    backgroundWindows.SelectMany(tr => data.ValuesForSensorInTimeRange(tr, sensorIndex)));
            return new MeanAndDeviation(Math.Min(descriptiveStatistics.Mean, 25.0),
                Math.Min(descriptiveStatistics.StandardDeviation, 5.0), descriptiveStatistics.Count);
        }

        private static Interval? DoFindOnsetEdges(
            IExamination data,
            int sensorIndex,
            Interval timeRange,
            IEnumerable<Interval> backgroundWindows,
            DiagnosticsContext diag)
        {
            dynamic baselineStats = CalculateBaselineStats(data, sensorIndex, backgroundWindows);
            double threshold = CalculateThreshold(baselineStats);
            diag.SetThreshold(sensorIndex, threshold);
            Logger.Trace(() => string.Format("FindSensorEdgesAboveBaseline #{0} Mean={1} StdDev={2} Threshold={3}",
                sensorIndex, baselineStats.Mean, baselineStats.StandardDeviation, threshold));
            Interval betweenTimes = timeRange;
            TimeSample sample = data.Samples.SamplesInTimeRange(timeRange).MaxBy(s => s.Values[sensorIndex]);
            Interval interval;
            while (true)
            {
                TimeSample sampleAtRise = FindSampleAtRise(data, sensorIndex, betweenTimes, threshold);
                if (sampleAtRise != null)
                {
                    betweenTimes = new Interval(sampleAtRise.Time, timeRange.End);
                    TimeSample sampleAtFall = FindSampleAtFall(data, sensorIndex, betweenTimes, threshold);
                    if (sampleAtFall == null || sampleAtRise.Time == sampleAtFall.Time)
                    {
                        Instant start = betweenTimes.Start + data.TickAmount();
                        if (start < betweenTimes.End)
                            betweenTimes = new Interval(start, betweenTimes.End);
                        else
                            goto label_8;
                    }
                    else
                    {
                        interval = new Interval(sampleAtRise.Time, sampleAtFall.Time);
                        if (!interval.Contains(sample.Time))
                            betweenTimes = new Interval(sampleAtFall.Time, timeRange.End);
                        else
                            break;
                    }
                }
                else
                {
                    goto label_8;
                }
            }

            return interval;
            label_8:
            return new Interval?();
        }

        private static TimeSample FindSampleAtRise(
            IExamination data,
            int sensorIndex,
            Interval betweenTimes,
            double threshold)
        {
            TimeSample sampleAtRise = null;
            foreach ((TimeSample sample, double slope) tuple in SamplesWithSlopeInTimeRange(data, sensorIndex, betweenTimes,
                         RiseSlopeWindowDuration))
                if (tuple.sample.Values[sensorIndex] > threshold)
                {
                    if (sampleAtRise == null)
                        sampleAtRise = tuple.sample;
                    if (tuple.slope > 0.4)
                        return tuple.sample;
                }

            return sampleAtRise;
        }

        private static TimeSample FindSampleAtFall(
            IExamination data,
            int sensorIndex,
            Interval betweenTimes,
            double threshold)
        {
            IEnumerator<TimeSample> enumerator = data.Samples.SamplesInTimeRange(betweenTimes).GetEnumerator();
            if (!enumerator.MoveNext())
                return null;
            TimeSample sampleAtFall = enumerator.Current;
            while (enumerator.MoveNext())
            {
                TimeSample current = enumerator.Current;
                if (current.Values[sensorIndex] > threshold)
                    sampleAtFall = current;
                else if (IsFalseTrough(sampleAtFall.Time, threshold, enumerator, sensorIndex))
                    sampleAtFall = enumerator.Current;
                else
                    break;
            }

            return sampleAtFall;
        }

        private static bool IsFalseTrough(
            Instant start,
            double threshold,
            IEnumerator<TimeSample> iterator,
            int sensorIndex)
        {
            while (iterator.MoveNext())
            {
                TimeSample current = iterator.Current;
                if (current.Values[sensorIndex] > threshold)
                    return true;
                if (current.Time - start > FalseTroughDetectionDuration)
                    break;
            }

            return false;
        }

        private static IEnumerable<(TimeSample sample, double slope)> SamplesWithSlopeInTimeRange(
            IExamination data,
            int sensorIndex,
            Interval betweenTimes,
            Duration slopeWindow)
        {

            int num1 = (int) (slopeWindow.ToMilliseconds() / data.TickAmount().ToMilliseconds()); // 100 / 10 = 10
            int indexAfter = num1 / 2;
            int indexBefore = num1 - indexAfter;
            for (int index = data.SampleIndexFromTime(betweenTimes.Start); index < data.Samples.Count; ++index)
            {
                var sample1 = data.Samples[index];
                if (sample1.Time >= betweenTimes.End)
                    break;
                var sample2 = data.Samples[index > indexBefore ? index - indexBefore : 0];
                var sample3 =
                    data.Samples[index + indexAfter < data.Samples.Count ? index + indexAfter : data.Samples.Count - 1];
            
                double num2 = sample3.Values[sensorIndex] - sample2.Values[sensorIndex];
                long num3 = sample3.Time.ToMillisecondsFromEpoch() - sample2.Time.ToMillisecondsFromEpoch();
                yield return (sample1, num2 / num3);
            }
        }

        private static double CalculateThreshold(dynamic stats)
        {
            double num = 2.0 * stats.StandardDeviation + 10.0;
            return stats.Mean + num;
        }

        private void Track(DiagnosticsContext diagnostics, int sensorIndex, Interval timeRange)
        {
            diagnostics.TrackEdges(timeRange, sensorIndex, DetectionMarkerType.TestWindow);
            foreach (Interval backgroundWindow in backgroundWindows)
                diagnostics.TrackEdges(backgroundWindow, sensorIndex, DetectionMarkerType.BackgroundWindow);
        }
    }
}