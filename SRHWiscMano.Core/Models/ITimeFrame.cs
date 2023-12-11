using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface ITimeFrame
    {
        string Text { get; set; }
        Instant Time { get; }
        double[,] PlotData { get; }
    }

    
}