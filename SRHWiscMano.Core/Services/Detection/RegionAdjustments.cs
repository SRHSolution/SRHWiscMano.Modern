using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    internal static class RegionAdjustments
    {
        public static Region SetTimeCenteredOnClickPoint(this Region region, Duration totalTime)
        {
            return region.SetTimeCenteredOnPoint(region.ClickPoint.Time, totalTime);
        }

        public static Region SetTimeCenteredOnPoint(
            this Region region,
            Instant timePoint,
            Duration totalTime)
        {
            Interval bounds = region.Window.ExamData.TotalTime();
            var interval = timePoint.AtCenterOfDuration(totalTime, bounds);
            return region.ChangeTime(interval);
        }

        public static Region SetTimeStartingAtPoint(
            this Region region,
            Instant timePoint,
            Duration totalTime)
        {
            Interval bounds = region.Window.TimeRange();
            return region.ChangeTime(timePoint.AtStartOfDuration(totalTime, bounds));
        }

        public static Region ExtendTimeLeft(this Region region, Duration timeBefore)
        {
            Interval extendedTimeRange = new Interval(region.TimeRange.Start - timeBefore, region.TimeRange.End);
            return region.ChangeTime(extendedTimeRange);
        }


        public static Region ShiftTime(this Region region, Duration amount)
        {
            Interval timeRange = region.TimeRange.Map(t => t + amount);
            return region.ChangeTime(timeRange);
        }

        public static Region AdjustToPointOfMaximumInCenter(this Region region)
        {
            Duration duration = region.TimeRange.Duration / 2L;
            return region.AdjustToPointOfMaximum(duration, duration);
        }

        public static Region AdjustToPointOfMaximumOnLeftEdge(this Region region)
        {
            return region.AdjustToPointOfMaximum(Duration.Zero, region.TimeRange.Duration);
        }

        public static Region AdjustToPointOfMaximumOnRightEdge(this Region region)
        {
            return region.AdjustToPointOfMaximum(region.TimeRange.Duration, Duration.Zero);
        }

        public static Region AdjustToPointOfMaximum(
            this Region region,
            Duration timeBefore,
            Duration timeAfter)
        {
            TimeSample sample = region.SampleWithMaximumOnClickedSensor();
            return region.ChangeTime(new Interval(sample.Time - timeBefore, sample.Time + timeAfter),
                new SamplePoint(sample.Time, region.ClickPoint.Sensor));
        }

        public static Region AdjustToTimeRangeAboveBaseline(
            this Region region,
            Duration backgroundWindowWidth,
            DiagnosticsContext diag)
        {
            return region.AdjustToTimeRangeAtEdges(new RangeAboveBaseline(backgroundWindowWidth, Duration.Zero), diag);
        }

        public static Region AdjustToTimeRangeAtEdges(
            this Region region,
            IEdgeAlgorithm algorithm,
            DiagnosticsContext diag)
        {
            Interval withTestWindow = RegionEdgeDetector.FindWithTestWindow(algorithm, region.Window.ExamData,
                region.SensorRange, region.TimeRange, region.Type, diag);
            return region.ChangeTime(withTestWindow);
        }
        
        public static Region AdjustToPeakChangeLeft(this Region region, Duration searchTimeBefore)
        {
            IExamination data = region.Window.ExamData;
            int sensor = region.ClickPoint.Sensor;
            Interval timeRange1 = region.TimeRange;
            Instant start1 = timeRange1.Start - searchTimeBefore;
            timeRange1 = region.TimeRange;
            Instant start2 = timeRange1.Start;
            Interval timeRange2 = new Interval(start1, start2);
            Duration offset = PeakChange.FindNegative(data, sensor, timeRange2).Time - region.TimeRange.Start;
            return region.ChangeTime(region.TimeRange.Map(t => t + offset));
        }
        
        public static Region AdjustToPeakChangeRight(this Region region, Duration searchTimeAfter)
        {
            IExamination data = region.Window.ExamData;
            int sensor = region.ClickPoint.Sensor;
            Interval timeRange1 = region.TimeRange;
            Instant end1 = timeRange1.End;
            timeRange1 = region.TimeRange;
            Instant end2 = timeRange1.End + searchTimeAfter;
            Interval timeRange2 = new Interval(end1, end2);
            Duration offsetRight = PeakChange.FindNegative(data, sensor, timeRange2).Time - region.TimeRange.End;
            return region.ChangeTime(region.TimeRange.Map(t => t + offsetRight));
        }

        public static Region ChangeTime(this Region region, Interval timeRange)
        {
            return region.ChangeTime(timeRange, region.FocalPoint);
        }

        public static Region ChangeTime(
            this Region region,
            Interval timeRange,
            SamplePoint focalPoint)
        {
            return new Region(region.Window, timeRange, region.SensorRange, region.Type, region.ClickPoint, focalPoint);
        }

        public static Region ChangeTimeAndSensors(
            this Region region,
            Interval timeRange,
            Range<int> sensorRange)
        {
            return new Region(region.Window, timeRange, sensorRange, region.Type, region.ClickPoint, region.FocalPoint);
        }
    }
}