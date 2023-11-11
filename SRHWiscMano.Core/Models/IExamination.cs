namespace SRHWiscMano.Core.Models
{
    public interface IExamination
    {
        IReadOnlyList<TimeSample> Samples { get; }
        IReadOnlyList<FrameNote> Notes { get; }
    }
}
