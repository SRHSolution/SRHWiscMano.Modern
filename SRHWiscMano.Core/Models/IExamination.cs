using System.Collections.ObjectModel;
using DynamicData;

namespace SRHWiscMano.Core.Models
{
    public interface IExamination
    {
        IReadOnlyList<TimeSample> Samples { get; }
        
        SourceList<FrameNote> Notes { get; }

        int InterpolationScale { get; }

        double[,] PlotData { get; }

        Task UpdatePlotData(int interpolateScale);
    }
}
