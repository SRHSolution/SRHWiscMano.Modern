using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface ITimeFrame
    {
        string Id { get; }

        IExamination Data { get; }

        string Text { get; }

        Instant Time { get; }

        Range<int> SensorRange { get; }

        int? VPUpperBound { get; }

        int? UesLowerBound { get; }

        bool IsSelected { get; }

        bool NormalEligible { get; }

        ITimeFrameLabels Labels { get; }

        IReadOnlyList<IRegion> Regions { get; }

        RegionsVersionType RegionsVersionType { get; }
    }

    
}