using MoreLinq;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class MaxCalculator
    {
        private const int SmoothingWindow = 3;

        /// <summary>
        /// 입력된 센서의 데이터를 Window average 낸 값들중에 Max를 찾는다
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="sensorIndex"></param>
        /// <returns></returns>
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