using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface IRegion
    {
        ITimeFrame Window { get; }

        Interval TimeRange { get; }

        Range<int> SensorRange { get; }

        IList<double> MaxValues { get; }
        IList<double> AvgValues { get; }

        RegionType Type { get; }

        SamplePoint ClickPoint { get; }

        SamplePoint FocalPoint { get; }
    }
}