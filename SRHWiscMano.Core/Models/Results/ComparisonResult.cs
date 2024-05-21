namespace SRHWiscMano.Core.Models.Results
{
    public class ComparisonResult
    {
        public ComparisonResult(
            SwallowResults<MeanAndDeviation> targetValues,
            SwallowResults<MeanDeviationOutlier> sourceOutliers)
        {
            TargetValues = targetValues;
            SourceOutliers = sourceOutliers;
        }

        public SwallowResults<MeanAndDeviation> TargetValues { get; }

        public SwallowResults<MeanDeviationOutlier> SourceOutliers { get; }
    }
}