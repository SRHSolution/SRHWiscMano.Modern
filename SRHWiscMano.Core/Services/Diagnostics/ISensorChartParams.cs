using NodaTime;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Diagnostics
{
    internal interface ISensorChartParams
    {
        ITimeFrame TimeFrame { get; }

        Interval TimeRange { get; }
    }
}