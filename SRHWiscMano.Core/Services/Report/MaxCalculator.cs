using MoreLinq;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class MaxCalculator
    {
        private const int SmoothingWindow = 3;

        public static double SmoothMax(this IEnumerable<TimeSample> samples, int sensorIndex)
        {
            return samples.Window(3).Select(ss => ss.Average(s => s.Values[sensorIndex])).Max();
        }

        public static double SmoothMin(this IEnumerable<TimeSample> samples, int sensorIndex)
        {
            return samples.Window(3).Select(ss => ss.Average(s => s.Values[sensorIndex])).Min();
        }
    }
}