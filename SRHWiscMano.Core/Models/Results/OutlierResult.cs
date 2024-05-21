namespace SRHWiscMano.Core.Models.Results
{
    public class OutlierResult
    {
        private readonly double deviation;
        private readonly bool isOutlier;
        private readonly double value;

        public OutlierResult(double value, double deviation, bool isOutlier)
        {
            this.value = value;
            this.deviation = deviation;
            this.isOutlier = isOutlier;
        }

        public double Value => value;

        public double Deviation => deviation;

        public bool IsOutlier => isOutlier;
    }
}