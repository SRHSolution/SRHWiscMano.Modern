using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface IRegion
    {
        ISnapshot Window { get; }

        Interval TimeRange { get; }

        Range<int> SensorRange { get; }

        RegionType Type { get; }

        DataPoint ClickPoint { get; }

        DataPoint FocalPoint { get; }
    }
}