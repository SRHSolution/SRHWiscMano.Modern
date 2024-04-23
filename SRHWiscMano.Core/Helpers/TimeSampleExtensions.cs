using Newtonsoft.Json;
using OxyPlot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using NLog;
using NodaTime;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Helpers
{
    public static class TimeSampleExtensions
    {
        /// <summary>
        /// TimeSample list를 double[,] 데이터 타입으로 변환한다.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="bFlipVertical"></param>
        /// <returns></returns>
        public static double[,] ConvertToDoubleArray(this IEnumerable<TimeSample> samples, bool bFlipVertical = false)
        {
            // Check if the input is empty or null to avoid IndexOutOfRangeException
            var sampleList = samples as IList<TimeSample> ?? samples.ToList();
            if (!sampleList.Any())
            {
                throw new ArgumentException("The input collection cannot be empty.", nameof(samples));
            }

            var sensorCount = (int)(sampleList[0].DataSize);
            var frameCount = sampleList.Count;

            // Initialize the 2D array
            var plotData = new double[frameCount, sensorCount];

            for (int frmId = 0; frmId < frameCount; frmId++)
            {
                var sensors = sampleList[frmId].Values;

                // Additional check to ensure that the sensor data matches the expected DataSize
                if (sensors.Count != sensorCount)
                {
                    throw new InvalidOperationException(
                        $"Inconsistent DataSize at frame {frmId}. Expected {sensorCount}, got {sensors.Count}.");
                }

                for (int senId = 0; senId < sensorCount; senId++)
                {
                    plotData[frmId, senId] = bFlipVertical ? sensors[sensorCount - senId - 1] : sensors[senId];
                }
            }

            return plotData;
        }


        /// <summary>
        /// Samples을 Interpolate 한뒤 결과를 반환한다.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="interpolateScale"></param>
        /// <returns></returns>
        public static List<TimeSample> InterpolateSamples(this IEnumerable<TimeSample> samples,
            int interpolateScale = 1)
        {
            var sampleList = samples.ToList();
            if (!sampleList.Any())
                return new List<TimeSample>(); // Return an empty list instead of null

            // 센서 사이의 값만 interpolation 한다.
            var sensorCount = (sampleList.First().DataSize-1) * interpolateScale + 1;

            // Use a concurrent collection to store results temporarily
            var tempResults = new ConcurrentBag<(int Index, TimeSample Sample)>();

            // var benchOld = new PerfBenchmarkActions();
            // var benchNew = new PerfBenchmarkActions();

            Parallel.For(0, sampleList.Count, i =>
            {
                double[] scaledSensorValues;
                if (interpolateScale != 1)
                {
                    // using (benchOld.AddAction())
                    {
                        scaledSensorValues =
                            Interpolators.LinearInterpolate(sampleList[i].Values.ToArray(), sensorCount);
                    }

                    // using(benchNew.AddAction())
                    // {
                    //     var values = sampleList[i].Values.ToArray().InterpolateTo(sensorCount);
                    //     scaledSensorValues = values.ToArray();
                    // }
                }
                else
                {
                    scaledSensorValues = sampleList[i].Values.ToArray();
                }

                var interpolatedSample = new TimeSample(sampleList[i].Time, scaledSensorValues);
                tempResults.Add((i, interpolatedSample));
            });

            // Debug.WriteLine($"InterpolateOld : {benchOld.CalcStatistics().Mean:F2}, {benchOld.CalcStatistics().StandardDeviation:F2}");
            //
            // Debug.WriteLine($"InterpolateNew : {benchNew.CalcStatistics().Mean:F2}, {benchNew.CalcStatistics().StandardDeviation:F2}");

            // Ensure the order of samples is preserved
            var orderedResults = tempResults.OrderBy(result => result.Index).Select(result => result.Sample).ToList();
            return orderedResults;
        }

        public static IEnumerable<double> ValuesForSensorInTimeRange(
            this IExamination data,
            Interval timeRange,
            int sensorIndex)
        {
            return data.Samples.SamplesInTimeRange(timeRange).Select((Func<TimeSample, double>)(s => s.Values[sensorIndex]));
        }


        public static IEnumerable<Tuple<TimeSample, double>> SampleValuesForSensorInTimeRange(
            this IExamination data,
            Interval timeRange,
            int sensorIndex)
        {
            return data.Samples.SamplesInTimeRange(timeRange)
                .Select((Func<TimeSample, Tuple<TimeSample, double>>)(s => Tuple.Create(s, s.Values[sensorIndex])));
        }


        public static TimeSample SamplesAtTime(this IEnumerable<TimeSample> samples,
            Instant time)
        {
            var subSamples = samples.FirstOrDefault(s => s.Time == time, null);
            return subSamples;
        }

        /// <summary>
        /// Time 간격에 대한 데이터를 추출하여 반환한다.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IReadOnlyList<TimeSample> SamplesInTimeRange(this IEnumerable<TimeSample> samples,
            Interval interval)
        {
            return samples.SamplesInTimeRange(interval.Start, interval.End);
        }

        /// <summary>
        /// Time 간격에 대한 데이터를 추출하여 반환한다.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static IReadOnlyList<TimeSample> SamplesInTimeRange(this IEnumerable<TimeSample> samples,
            Instant startTime, Instant endTime)
        {
            var subSamples = samples.Where(s => s.Time >= startTime && s.Time < endTime);
            return subSamples.ToList().AsReadOnly();
        }


        /// <summary>
        /// TimeSamples 에서 특정 센서 영역의 값만 추출하여 새로운 TimeSamples로 전달한다
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="sensorRange"></param>
        /// <returns></returns>
        public static IReadOnlyList<TimeSample> SamplesForSensorRange(this IEnumerable<TimeSample> samples,
            Range<int> sensorRange)
        {
            var newSample = samples.Select(ts =>
            {
                var rangeValue = Enumerable.Range(sensorRange.Start, sensorRange.Span())
                    .Select(idx => ts.Values[idx]).ToList().AsReadOnly();
                return new TimeSample(ts.Time, rangeValue);
            });
            
            return newSample.ToList().AsReadOnlyList();
        }

    }
}