using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services.Report
{
    public static class AggregateCalculator
    {
        public static SwallowResults<MeanAndDeviation> CalculateAggregate(
            this IEnumerable<SwallowResults<double>> individualResults)
        {
            SwallowResults<double> swallowResults = individualResults.FirstOrDefault();
            SwallowResults<MeanAndDeviation> aggregate = ResultsEngine.Generate(
                (Func<Func<SwallowResults<double>, double>, MeanAndDeviation>) (selector =>
                    individualResults.Select(selector).ToMeanAndDeviation()));
            aggregate.MaxPressures = individualResults.Select(r => r.MaxPressures).MatrixAggregate().ToList();
            aggregate.PressureAtVPMax = individualResults.Select(r => r.PressureAtVPMax).MatrixAggregate().ToList();
            aggregate.PressureAtTBMax = individualResults.Select(r => r.PressureAtTBMax).MatrixAggregate().ToList();
            aggregate.PressureGradient = individualResults.Select(r => r.PressureGradient).MatrixAggregate().ToList();
            return aggregate;
        }

        public static SwallowResults<double> ToIndividualForMean(
            this SwallowResults<MeanAndDeviation> aggregateResults)
        {
            SwallowResults<double> individualForMean = ResultsEngine.Generate(
                (Func<Func<SwallowResults<MeanAndDeviation>, MeanAndDeviation>, double>) (selector =>
                    selector(aggregateResults).Mean));
            individualForMean.MaxPressures = aggregateResults.MaxPressures.Select(md => md.Mean).ToList();
            individualForMean.PressureAtVPMax = aggregateResults.PressureAtVPMax.Select(md => md.Mean).ToList();
            individualForMean.PressureAtTBMax = aggregateResults.PressureAtTBMax.Select(md => md.Mean).ToList();
            individualForMean.PressureGradient = aggregateResults.PressureGradient.Select(md => md.Mean).ToList();
            return individualForMean;
        }

        public static SwallowResults<MeanDeviationOutlier> Combine(
            SwallowResults<MeanAndDeviation> aggResults,
            SwallowResults<OutlierResult> aggOutliers)
        {
            SwallowResults<MeanDeviationOutlier> swallowResults = ResultsEngine.Combine(aggResults, aggOutliers,
                (a, o) => new MeanDeviationOutlier(a, o.IsOutlier));
            swallowResults.MaxPressures =
                aggResults.MaxPressures.Select(a => new MeanDeviationOutlier(a, false)).ToList();
            swallowResults.PressureAtVPMax =
                aggResults.PressureAtVPMax.Select(a => new MeanDeviationOutlier(a, false)).ToList();
            swallowResults.PressureAtTBMax =
                aggResults.PressureAtTBMax.Select(a => new MeanDeviationOutlier(a, false)).ToList();
            swallowResults.PressureGradient =
                aggResults.PressureGradient.Select(a => new MeanDeviationOutlier(a, false)).ToList();
            return swallowResults;
        }

        public static SwallowResults<MeanDeviationOutlier> AsMeanDeviationOutlier(
            this SwallowResults<MeanAndDeviation> results)
        {
            SwallowResults<MeanDeviationOutlier> swallowResults = ResultsEngine.Generate(
                (Func<Func<SwallowResults<MeanAndDeviation>, MeanAndDeviation>, MeanDeviationOutlier>) (selector =>
                    new MeanDeviationOutlier(selector(results), false)));
            swallowResults.MaxPressures = results.MaxPressures.Select(a => new MeanDeviationOutlier(a, false)).ToList();
            swallowResults.PressureAtVPMax =
                results.PressureAtVPMax.Select(a => new MeanDeviationOutlier(a, false)).ToList();
            swallowResults.PressureAtTBMax =
                results.PressureAtTBMax.Select(a => new MeanDeviationOutlier(a, false)).ToList();
            swallowResults.PressureGradient =
                results.PressureGradient.Select(a => new MeanDeviationOutlier(a, false)).ToList();
            return swallowResults;
        }

        internal static IEnumerable<MeanAndDeviation> MatrixAggregate(
            this IEnumerable<IEnumerable<double>> matrix)
        {
            return matrix.Inverse().Select(r => r.ToMeanAndDeviation());
        }
    }
}