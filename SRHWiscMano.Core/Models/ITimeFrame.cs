using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface ITimeFrame
    {
        string Id { get; }
        string Text { get; set; }
        Instant Time { get; }
        double TimeDuration { get; }

        double[,] PlotData { get; }

        void UpdateTime(Instant newTime);
    }
}