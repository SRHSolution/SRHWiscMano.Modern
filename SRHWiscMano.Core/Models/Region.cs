using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public class Region : IRegion
    {
        public Region(
            ISnapshot window,
            Interval timeRange,
            Range<int> sensorRange,
            RegionType type,
            DataPoint clickPoint)
        {
            Window = window;
            TimeRange = timeRange;
            SensorRange = sensorRange;
            Type = type;
            ClickPoint = clickPoint;
        }

        public Region(
            ISnapshot window,
            Interval timeRange,
            Range<int> sensorRange,
            RegionType type,
            DataPoint clickPoint,
            DataPoint focalPoint)
            : this(window, timeRange, sensorRange, type, clickPoint)
        {
            FocalPoint = focalPoint;
        }

        public ISnapshot Window { get; set; }

        public Interval TimeRange { get; set; }

        public Range<int> SensorRange { get; set; }

        public RegionType Type { get; set; }

        public DataPoint ClickPoint { get; set; }

        public DataPoint FocalPoint { get; set; }
    }
}