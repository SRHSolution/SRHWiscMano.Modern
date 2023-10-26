using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface ISnapshot
    {
        string Id { get; }

        IExamData Data { get; }

        string Text { get; }

        Instant Time { get; }

        Range<int> SensorRange { get; }

        int? VPUpperBound { get; }

        int? UesLowerBound { get; }

        bool IsSelected { get; }

        bool NormalEligible { get; }

        ISnapshotLabels Labels { get; }

        IReadOnlyList<IRegion> Regions { get; }

        RegionsVersion RegionsVersion { get; }
    }

    
}