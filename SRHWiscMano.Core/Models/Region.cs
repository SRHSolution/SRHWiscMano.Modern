using NodaTime;
using SRHWiscMano.Core.Helpers;

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
    }
}