using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace SRHWiscMano.Core.Helpers
{
    public class PerfBenchmarkActions
    {
        private List<double> timeLapseList = new List<double>();

        public BenchmarkAction AddAction()
        {
            return new BenchmarkAction(timeLapseList);
        }

        /// <summary>
        /// 평균 시간을 micro sec로 반환한다.
        /// </summary>
        /// <param name="rejectOutliers"></param>
        /// <returns></returns>
        public (double Mean, double StandardDeviation) CalcStatistics()
        {
            var max = Statistics.Maximum(timeLapseList);
            var min = Statistics.Minimum(timeLapseList);
            var variance = Statistics.Variance(timeLapseList);
            var mean = Statistics.Mean(timeLapseList);
            var std = Statistics.StandardDeviation(timeLapseList);
            var upperQuartile = Statistics.UpperQuartile(timeLapseList);
            var lowerQuartile = Statistics.LowerQuartile(timeLapseList);
            return Statistics.MeanStandardDeviation(timeLapseList.Select(lapse => (double)lapse));
        }

       
    }

    public class BenchmarkAction : IDisposable
    {
        private readonly List<double> timeLapseList;
        private readonly Stopwatch stopwatch;

        public BenchmarkAction(List<double> timeLapseList)
        {
            this.timeLapseList = timeLapseList;
            stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            timeLapseList.Add(CalcToTimeInUs(stopwatch.ElapsedTicks));
        }
        private double CalcToTimeInUs(long tick)
        {
            return tick / 10.0;
        }
    }
}
