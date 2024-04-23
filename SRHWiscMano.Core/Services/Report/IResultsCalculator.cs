using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services.Report
{
    public interface IResultsCalculator
    {
        SwallowResults<double> CalculateIndividual(ITimeFrame snapshot);

        SwallowResults<MeanAndDeviation> CalculateAggregate(
            IEnumerable<SwallowResults<double>> individualResults);

        IEnumerable<SwallowResults<OutlierResult>> OutliersFromAggregate(
            IEnumerable<SwallowResults<double>> individualResults,
            SwallowResults<MeanAndDeviation> aggregateResults);

        SwallowResults<MeanDeviationOutlier> AggregateOutliersFromComparison(
            SwallowResults<MeanAndDeviation> aggregateResults,
            SwallowResults<MeanAndDeviation> comparison);
    }
}