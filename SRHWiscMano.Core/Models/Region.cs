using NodaTime;
using SkiaSharp;
using SRHWiscMano.Core.Helpers;
using System.Text;

namespace SRHWiscMano.Core.Models
{
    public class Region : IRegion
    {
        public Region(
            ITimeFrame window,
            Interval timerange,
            Range<int> sensorRange,
            RegionType type,
            SamplePoint clickPoint)
        {
            Window = window;
            TimeRange = timerange;
            SensorRange = sensorRange;
            Type = type;
            ClickPoint = clickPoint;
        }

        public Region(
            ITimeFrame window,
            Interval timeRange,
            Range<int> sensorRange,
            RegionType type,
            SamplePoint clickPoint,
            SamplePoint focalPoint)
            : this(window, timeRange, sensorRange, type, clickPoint)
        {
            FocalPoint = focalPoint;
        }

        public ITimeFrame Window { get; set; }

        public Interval TimeRange { get; set; }

        public Range<int> SensorRange { get; set; }

        public RegionType Type { get; set; }

        public SamplePoint ClickPoint { get; set; }

        public SamplePoint FocalPoint { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Region : " + Type);
            sb.AppendLine($"Sensor Range : {SensorRange.Start}-{SensorRange.End}");
            sb.AppendLine(
            $"Time Interval : {TimeRange.Start.ToUnixTimeMilliseconds()}-{TimeRange.End.ToUnixTimeMilliseconds()}");
            sb.AppendLine(
            $"Click Point : {ClickPoint.Sensor}, {ClickPoint.Time.ToUnixTimeMilliseconds()}");
            sb.AppendLine(
            $"Focal Point : {FocalPoint?.Sensor}, {FocalPoint?.Time.ToUnixTimeMilliseconds()}");

            return sb.ToString();
        }
    }
}