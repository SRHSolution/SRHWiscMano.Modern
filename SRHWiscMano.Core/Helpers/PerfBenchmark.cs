using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Helpers
{
    public class PerfBenchmark : IDisposable
    {
        public enum TIME_MODE{
            TICK,
            MS,
            US,
            NS
        }

        struct PerfCheckPoint
        {
            public PerfCheckPoint(long timelapse):this("", timelapse)
            {
            }

            public PerfCheckPoint(string name, long timelapse)
            {
                this.timelapse = timelapse;
                this.name = name;
            }

            public string name;
            public long timelapse;
        }

        private readonly Stopwatch stopwatch;
        private readonly List<PerfCheckPoint> checkpoints;
        public string TaskName { get; }
        public TIME_MODE TimeMode { get; set; } = TIME_MODE.MS;


        public PerfBenchmark(string taskName)
        {
            TaskName = taskName;
            checkpoints = new List<PerfCheckPoint>();
            stopwatch = Stopwatch.StartNew();
        }

        public void Start()
        {
            stopwatch.Reset();
            stopwatch.Start();
        }

        public void Stop()
        {
            Stop($"chk{checkpoints.Count}");
        }

        public void Stop(string chkName)
        {
            checkpoints.Add(new PerfCheckPoint(chkName, stopwatch.ElapsedTicks));
            stopwatch.Reset();
        }


        public void SaveCheckpoint()
        {
            SaveCheckpoint($"chk{checkpoints.Count}");
        }

        public void SaveCheckpoint(string chkName)
        {
            checkpoints.Add(new PerfCheckPoint(chkName,stopwatch.ElapsedTicks));
            stopwatch.Reset();
            stopwatch.Start();
        }

        public void ClearCheckpoints()
        {
            checkpoints.Clear();
        }

        public List<string> GetCheckPointsInfos()
        {
            return checkpoints.ConvertAll(chk => $"{chk.name}:{CalcToTime(chk.timelapse)}");
            // return checkpoints.Select(chk => $"{chk.name}:{calcToTime(chk.timelapse)}").ToList();
        }


        public void Dispose()
        {
            stopwatch.Stop();
            Console.WriteLine($"{TaskName} took {CalcToTime(checkpoints.Select(chk=>chk.timelapse).Sum())} {TimeMode} for {checkpoints.Count} times");
        }
        

        public double CalculateAverage(bool rejectOutliers = true)
        {
            List<PerfCheckPoint> inliers = checkpoints;
            if (rejectOutliers)
            {
                var inlierIdx = RemoveOutliersIQR(checkpoints.Select(s => (double)s.timelapse).ToList());
                inliers = inlierIdx.Select(i => checkpoints[i]).ToList();
            }

            double sum = inliers.Aggregate<PerfCheckPoint, double>(0, (current, checkpoint) => current + checkpoint.timelapse);
            var res = sum / checkpoints.Count;

            return CalcToTime(res);
        }

        public double CalculateStandardDeviation(bool rejectOutliers = true)
        {
            List<PerfCheckPoint> inliers = checkpoints;
            if (rejectOutliers)
            {
                var inlierIdx = RemoveOutliersIQR(checkpoints.Select(s => (double)s.timelapse).ToList());
                inliers = inlierIdx.Select(i => checkpoints[i]).ToList();
            }

            var sum = inliers.Aggregate<PerfCheckPoint, double>(0, (current, checkpoint) => current + checkpoint.timelapse);
            var average = sum / checkpoints.Count;

            var sumOfSquaresOfDifferences = inliers.Sum(checkpoint => (checkpoint.timelapse - average) * (checkpoint.timelapse - average));
            var res = Math.Sqrt(sumOfSquaresOfDifferences / checkpoints.Count);

            return CalcToTime(res);
        }

        /// <summary>
        /// Benchmark 성능 평가를 위한 outlier 제거 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<int> RemoveOutliersIQR(IReadOnlyCollection<double> data)
        {
            double Q1 = data.OrderBy(x => x).Skip((int)(0.25 * data.Count)).First();
            double Q3 = data.OrderBy(x => x).Skip((int)(0.75 * data.Count)).First();

            double IQR = Q3 - Q1;
            double lowerBound = Q1 - 1.5 * IQR;
            double upperBound = Q3 + 1.5 * IQR;

            return data.Select((x, i) => new { Index = i, Value = x })
                .Where(x => (x.Value >= lowerBound) && (x.Value <= upperBound))
                .Select(x => x.Index)
                .ToList();
        }

        /// <summary>
        /// 설정된 시간 표현으로 변경
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        private double CalcToTime(double tick)
        {
            switch (TimeMode)
            {
                case TIME_MODE.TICK:
                    return tick;

                // 1 tick = 100 nano sec
                case TIME_MODE.NS: 
                    return tick * 100;

                case TIME_MODE.US:
                    return tick / 10.0;

                case TIME_MODE.MS:
                    return tick / 10000.0;
            }

            return tick;
        }
    }
}
