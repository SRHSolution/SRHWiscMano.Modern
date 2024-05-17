using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Services.Report
{
    public static class TimeFrameResultsCalculator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultsCalc"></param>
        /// <param name="snapshots"></param>
        /// <returns></returns>
        public static ExamResults<OutlierResult> CalculateSnapshotResults(
            this IResultsCalculator resultsCalc,
            IEnumerable<ITimeFrame> snapshots)
        {
            // snapshopt의 region 에 대한 계산을수행한다. 실제로 RegionCalculator를 사용함
            List<Tuple<ITimeFrame, SwallowResults<double>>> list =
                snapshots.Select(s => Tuple.Create(s, resultsCalc.CalculateIndividual(s))).ToList();
            IEnumerable<SwallowResults<double>> individualResults = list.Select(ir => ir.Item2);
            SwallowResults<MeanAndDeviation> aggregate = resultsCalc.CalculateAggregate(individualResults);
            return new ExamResults<OutlierResult>(
                // list 의 아이템과 resultsCalc.OutliersFromAggregate 결과 값을, 마지막 function 을 이용해서 zip 을 수행하도록 한다.
                list.Zip(resultsCalc.OutliersFromAggregate(individualResults, aggregate),
                    (ir, or) => new TimeFrameResult<OutlierResult>(ir.Item1, or)).ToList(), aggregate);
        }

        public static ComparisonResult CalculateComparisonOutliers(
            this IResultsCalculator resultsCalc,
            SwallowResults<MeanAndDeviation> aggregateResults,
            SwallowResults<MeanAndDeviation> comparison)
        {
            SwallowResults<MeanDeviationOutlier> sourceOutliers =
                resultsCalc.AggregateOutliersFromComparison(aggregateResults, comparison);
            return new ComparisonResult(comparison, sourceOutliers);
        }
    }
}