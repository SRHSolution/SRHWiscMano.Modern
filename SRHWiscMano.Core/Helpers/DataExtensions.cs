using MathNet.Numerics.Statistics;
using MoreLinq;
using SRHWiscMano.Core.Models.Results;

namespace SRHWiscMano.Core.Helpers
{
    public static class DataExtensions
    {
        public static MeanAndDeviation ToMeanAndDeviation(this IEnumerable<double> data)
        {
            DescriptiveStatistics descriptiveStatistics = new DescriptiveStatistics(data);
            return new MeanAndDeviation(descriptiveStatistics.Mean, descriptiveStatistics.StandardDeviation,
                descriptiveStatistics.Count);
        }

        public static IEnumerable<double> Differences(this IEnumerable<double> data)
        {
            return data.Pairwise((a, b) => b - a);
        }

        public static IEnumerable<IEnumerable<double>> Inverse(
            this IEnumerable<IEnumerable<double>> matrix)
        {
            List<IEnumerator<double>> iterators = matrix.Select(row => row.GetEnumerator()).ToList();
            if (iterators.Count != 0)
                while (iterators.Select(it => it.MoveNext()).All(more => more))
                    yield return iterators.Select(it => it.Current);
        }

        public static int RoundToNearest(this double value, int multiple)
        {
            return (int) Math.Round(value / multiple) * multiple;
        }

        public static IEnumerable<double> Filter(
            this IEnumerable<double> input,
            double[] kernel,
            Func<double, double, double> filterFunc)
        {
            return input.Window(kernel.Length).Select(window => window.Zip(kernel, filterFunc).Sum());
        }
    }
}