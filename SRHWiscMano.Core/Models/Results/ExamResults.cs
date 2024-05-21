namespace SRHWiscMano.Core.Models.Results
{
    public class ExamResults<T>
    {
        public ExamResults(
            IReadOnlyList<TimeFrameResult<T>> individuals,
            SwallowResults<MeanAndDeviation> aggregate)
        {
            Individuals = individuals;
            Aggregate = aggregate;
        }

        public IReadOnlyList<TimeFrameResult<T>> Individuals { get; }

        public SwallowResults<MeanAndDeviation> Aggregate { get; }
    }
}