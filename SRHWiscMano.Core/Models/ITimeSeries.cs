using NodaTime;

namespace SRHWiscMano.Core.Models
{
    internal interface ITimeSeries
    {
        Instant Time { get;  }
        IReadOnlyList<double> Values { get; }
    }
}
