using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    internal static class VPSensorDetection
    {
        private static readonly Duration FalsePeakDetectionDuration = Duration.FromMilliseconds(100L);

        public static Region DetermineVPBounds(
            this Region initialRegion,
            IEdgeAlgorithm edgeAlgorithm,
            DiagnosticsContext diag)
        {
            return initialRegion.DetermineVPAndTBSensorRange(edgeAlgorithm, diag);
            // return versionType == RegionsVersionType.UsesMP
                // ? initialRegion.AdjustToTimeRangeAtEdges(edgeAlgorithm, diag)
                // : initialRegion.DetermineVPAndTBSensorRange(edgeAlgorithm, diag) ??
                  // initialRegion.AdjustToTimeRangeAtEdges(edgeAlgorithm, diag);
        }


        private static Region DetermineVPAndTBSensorRange(
            this Region initialRegion,
            IEdgeAlgorithm edgeAlgorithm,
            DiagnosticsContext diag)
        {
            Range<int> range = initialRegion.SensorRange.SetSpan(4);
            List<Interval> list = RegionEdgeDetector
                .FindValidEdges(edgeAlgorithm, initialRegion.Window.ExamData, range, initialRegion.TimeRange, diag)
                .ToList();
            if (list.Count != range.Span())
                return null;
            Interval interval = list[3];
            Instant start1 = interval.Start;
            interval = list[2];
            Instant start2 = interval.Start;
            if (start1 - start2 >= Duration.FromMilliseconds(200L))
                return TakeTopSensors(initialRegion, list, 3);
            interval = list[2];
            Instant start3 = interval.Start;
            interval = list[1];
            Instant start4 = interval.Start;
            if (start3 - start4 >= Duration.FromMilliseconds(200L))
                return TakeTopSensors(initialRegion, list, 2);
            var firstPeak1 = FindFirstPeak(initialRegion.Window.FrameSamples, list[1], range.Start + 1);
            if (firstPeak1 == null)
                return null;
            var firstPeak2 = FindFirstPeak(initialRegion.Window.FrameSamples, list[2], range.Start + 2);
            if (firstPeak2 == null)
                return null;
            diag.AddMarker(firstPeak1.Time, range.Start + 1, DetectionMarkerType.FirstPeak);
            diag.AddMarker(firstPeak2.Time, range.Start + 2, DetectionMarkerType.FirstPeak);
            return firstPeak2.Time - firstPeak1.Time >= Duration.FromMilliseconds(100L)
                ? TakeTopSensors(initialRegion, list, 2)
                : TakeTopSensors(initialRegion, list, 3);
        }

        private static TimeSample FindFirstPeak(
            IReadOnlyList<TimeSample> samples,
            Interval timeRange,
            int sensorIndex)
        {
            IEnumerator<TimeSample> enumerator = samples.SamplesInTimeRange(timeRange).GetEnumerator();
            if (!enumerator.MoveNext())
                return null;
            var peak = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current.Values[sensorIndex] >= peak.Values[sensorIndex])
                    peak = current;
                else if (IsFalsePeak(peak, enumerator, sensorIndex))
                    peak = enumerator.Current;
                else
                    break;
            }

            return peak;
        }

        private static bool IsFalsePeak(TimeSample peak, IEnumerator<TimeSample> iterator, int sensorIndex)
        {
            while (iterator.MoveNext())
            {
                TimeSample current = iterator.Current;
                if (current.Values[sensorIndex] > peak.Values[sensorIndex])
                    return true;
                if (current.Time - peak.Time > FalsePeakDetectionDuration)
                    break;
            }

            return false;
        }

        private static Region TakeTopSensors(Region region, List<Interval> edges, int n)
        {
            return region.ChangeTimeAndSensors(RegionEdgeDetector.TimeRangeFromEdges(edges.Take(n)),
                region.SensorRange.SetSpan(n));
        }
    }
}