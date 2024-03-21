using System.Collections.ObjectModel;
using DynamicData;

namespace SRHWiscMano.Core.Models
{
    public interface IExamination
    {
        IReadOnlyList<TimeSample> Samples { get; }
        IReadOnlyList<TimeSample> InterpolatedSamples { get; }

        IReadOnlyList<FrameNote> Notes { get; }

        int InterpolationScale { get; }

        void UpdateInterpolation(int interpolateScale);
    }
}
