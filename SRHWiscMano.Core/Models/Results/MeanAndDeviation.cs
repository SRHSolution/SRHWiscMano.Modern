namespace SRHWiscMano.Core.Models.Results
{
    public class MeanAndDeviation
    {
        private readonly long count;
        private readonly double mean;
        private readonly double standardDeviation;

        public MeanAndDeviation(double mean, double standardDeviation, long count)
        {
            this.mean = mean;
            this.standardDeviation = standardDeviation;
            this.count = count;
        }

        public double Mean => mean;

        public double StandardDeviation => standardDeviation;

        public long Count => count;
    }
}