namespace SRHWiscMano.Core.Models
{
    public interface IExamination
    {
        IReadOnlyList<Sample> Samples { get; }
        IReadOnlyList<Note> Notes { get; }
    }
}
