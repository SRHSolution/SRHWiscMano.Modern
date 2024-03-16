using Newtonsoft.Json;
using OxyPlot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static double[,] ConvertToDoubleArray(this IEnumerable<TimeSample> samples, bool bFlipVertical = true)
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
                    throw new InvalidOperationException($"Inconsistent DataSize at frame {frmId}. Expected {sensorCount}, got {sensors.Count}.");
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
        public static List<TimeSample> InterpolateSamplesOld(this IEnumerable<TimeSample> samples, int interpolateScale = 1)
        {
            var sampleList = samples.ToList();
            if (sampleList.Count == 0)
                return null;

            var sensorCount = (int)(sampleList.First().DataSize * interpolateScale);
            var frameCount = sampleList.Count();

            var interpolateSamples = new List<TimeSample>();
            Parallel.For(0, frameCount, i =>
            {
                double[] scaledSensorValues = { };
                if (interpolateScale != 1)
                {
                    scaledSensorValues =
                        Interpolators.LinearInterpolate(sampleList[i].Values.ToArray(), sensorCount);
                }
                else
                {
                    scaledSensorValues = sampleList[i].Values.ToArray();
                }

                interpolateSamples.Add(new TimeSample(sampleList[i].Time, scaledSensorValues.ToList().AsReadOnly()));
            });

            return interpolateSamples;
        }

        /// <summary>
        /// Samples을 Interpolate 한뒤 결과를 반환한다.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="interpolateScale"></param>
        /// <returns></returns>
        public static List<TimeSample> InterpolateSamples(this IEnumerable<TimeSample> samples, int interpolateScale = 1)
        {
            var sampleList = samples.ToList();
            if (!sampleList.Any())
                return new List<TimeSample>(); // Return an empty list instead of null

            var sensorCount = sampleList.First().DataSize * interpolateScale;

            // Use a concurrent collection to store results temporarily
            var tempResults = new ConcurrentBag<(int Index, TimeSample Sample)>();

            Parallel.For(0, sampleList.Count, i =>
            {
                double[] scaledSensorValues;
                if (interpolateScale != 1)
                {
                    scaledSensorValues = Interpolators.LinearInterpolate(sampleList[i].Values.ToArray(), sensorCount);
                }
                else
                {
                    scaledSensorValues = sampleList[i].Values.ToArray();
                }

                var interpolatedSample = new TimeSample(sampleList[i].Time, scaledSensorValues);
                tempResults.Add((i, interpolatedSample));
            });

            // Ensure the order of samples is preserved
            var orderedResults = tempResults.OrderBy(result => result.Index).Select(result => result.Sample).ToList();
            return orderedResults;
        }


        /// <summary>
        /// Time 간격에 대한 데이터를 추출하여 반환한다.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static IReadOnlyList<TimeSample> GetSubSamples(this IEnumerable<TimeSample> samples, Instant startTime, Instant endTime)
        {
            var subSamples = samples.Where(s => s.Time >= startTime && s.Time <= endTime);
            return subSamples.ToList().AsReadOnly();
        }
    }
}
