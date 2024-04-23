using System.Collections.ObjectModel;
using DynamicData;
using NodaTime;

namespace SRHWiscMano.Core.Models
{
    public interface IExamination
    {
        IReadOnlyList<TimeSample> Samples { get; }
        IReadOnlyList<TimeSample> InterpolatedSamples { get; }

        IReadOnlyList<FrameNote> Notes { get; }

        int InterpolationScale { get; }
        string Title { get; }
        string Id { get; }

        void UpdateInterpolation(int interpolateScale);
    }
}
