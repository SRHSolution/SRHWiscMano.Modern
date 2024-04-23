namespace SRHWiscMano.Core.Models.Results
{
    public class MeanDeviationOutlier : MeanAndDeviation
    {
        private readonly bool isOutlier;

        public MeanDeviationOutlier(MeanAndDeviation meanAndDev, bool isOutlier)
            : base(meanAndDev.Mean, meanAndDev.StandardDeviation, meanAndDev.Count)
        {
            this.isOutlier = isOutlier;
        }

        public bool IsOutlier => isOutlier;
    }
}