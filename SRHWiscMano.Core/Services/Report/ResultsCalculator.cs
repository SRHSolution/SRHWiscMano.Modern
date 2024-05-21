using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services.Report
{
    /// <summary>
    /// 입력된 TimeFrame을 단독적으로 게산을 수행한다.
    /// </summary>
    public class ResultsCalculator : IResultsCalculator
    {
        public SwallowResults<double> CalculateIndividual(ITimeFrame snapshot)
        {
            return RegionCalculator.CalcResults(snapshot);
        }

        public SwallowResults<MeanAndDeviation> CalculateAggregate(
            IEnumerable<SwallowResults<double>> individualResults)
        {
            return individualResults.CalculateAggregate();
        }

        public IEnumerable<SwallowResults<OutlierResult>> OutliersFromAggregate(
            IEnumerable<SwallowResults<double>> individualResults,
            SwallowResults<MeanAndDeviation> aggregateResults)
        {
            return individualResults.CalculateOutliersFromAggregate(aggregateResults);
        }

        public SwallowResults<MeanDeviationOutlier> AggregateOutliersFromComparison(
            SwallowResults<MeanAndDeviation> aggregateResults,
            SwallowResults<MeanAndDeviation> comparison)
        {
            SwallowResults<OutlierResult> outlierFromAggregate =
                aggregateResults.ToIndividualForMean().CalculateOutlierFromAggregate(comparison);
            return AggregateCalculator.Combine(aggregateResults, outlierFromAggregate);
        }
    }
}