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

        /// <summary>
        /// 개별 값과 누적값의 평균, 표준편차를 통해서 outlier 가 되었는지를 확인한다.
        /// Threshold는 ICDF에서 얻은 표준편차에 비례된 정균분포 값입니다.
        /// </summary>
        /// <param name="individual"></param>
        /// <param name="aggregate"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
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

        /// <summary>
        /// ICDF 역누적분포를 통해 역누적 분포 함수는 특정 확률에 해당하는 값을 계산합니다.
        /// 즉, 주어진 확률 p에 대해, 이 확률이 나타나는 정규 분포상의 값을 반환합니다.
        /// 이것은 종종 확률 변수가 특정 확률에 해당하는 값을 찾는 데 사용됩니다.
        /// 샘플 크기가 클수록 임계값이 작아지게 되어, 더 많은 데이터 포인트가 아웃라이어로 간주될 수 있습니다.
        /// 이는 통계적으로 작은 샘플에서 발생할 수 있는 이상치를 적절히 처리하고, 큰 샘플에서는 더욱 엄격한 기준을 적용하기 위함입니다.
        /// https://kr.mathworks.com/help/stats/norminv.html
        /// </summary>
        /// <param name="sampleSize"></param>
        /// <returns></returns>
        private static double FractionalDeviationThreshold(long sampleSize)
        {
            return Math.Abs(new Normal().InverseCumulativeDistribution(1.0 / (4.0 * sampleSize)));
        }
    }
}