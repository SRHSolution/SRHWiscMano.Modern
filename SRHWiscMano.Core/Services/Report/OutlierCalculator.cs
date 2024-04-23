using MathNet.Numerics.Distributions;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class OutlierCalculator
    {
        public static IEnumerable<SwallowResults<OutlierResult>> CalculateOutliers(
            this IEnumerable<SwallowResults<double>> individualResults)
        {
            return individualResults.CalculateOutliersFromAggregate(individualResults.CalculateAggregate());
        }

        public static IEnumerable<SwallowResults<OutlierResult>> CalculateOutliersFromAggregate(
            this IEnumerable<SwallowResults<double>> individualResults,
            SwallowResults<MeanAndDeviation> aggregateResults)
        {
            double threshold = FractionalDeviationThreshold(aggregateResults.VP.Duration.Count);
            return individualResults.Select(ir => CreateResults(ir, aggregateResults, threshold));
        }

        public static SwallowResults<OutlierResult> CalculateOutlierFromAggregate(
            this SwallowResults<double> individualResults,
            SwallowResults<MeanAndDeviation> aggregateResults)
        {
            double threshold = FractionalDeviationThreshold(aggregateResults.VP.Duration.Count);
            return CreateResults(individualResults, aggregateResults, threshold);
        }

        private static SwallowResults<OutlierResult> CreateResults(
            SwallowResults<double> individualResults,
            SwallowResults<MeanAndDeviation> aggregateResults,
            double threshold)
        {
            return ResultsEngine.Combine(individualResults, aggregateResults, (i, a) => CalcOutlier(i, a, threshold));
        }

        private static OutlierResult CalcOutlier(
            double individual,
            MeanAndDeviation aggregate,
            double threshold)
        {
            if (aggregate == null)
                return new OutlierResult(individual, double.NaN, false);
            double deviation = Math.Abs(individual - aggregate.Mean) / aggregate.StandardDeviation;
            bool isOutlier = deviation > threshold;
            return new OutlierResult(individual, deviation, isOutlier);
        }

        private static double FractionalDeviationThreshold(long sampleSize)
        {
            return Math.Abs(new Normal().InverseCumulativeDistribution(1.0 / (4.0 * sampleSize)));
        }
    }
}