using MoreLinq;
using NLog;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    internal class PeakChange : IEdgeAlgorithm
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Interval? FindOnsetEdges(
            IExamination data,
            int sensorIndex,
            Interval timeRange,
            DiagnosticsContext diagnostics)
        {
            int size = 5;
            List<Tuple<TimeSample, double>> values = data.SampleValuesForSensorInTimeRange(timeRange, sensorIndex).ToList();
            Logger.Debug(() => string.Format("FindSensorEdges sensorIndex={0} {1}", sensorIndex,
                string.Join(",", values.Select(v => v.Item2))));
            if (values.Count < size * 2)
                return new Interval?();
            int num1 = size / 2;
            IEnumerable<double> source1 = values.Select(v => v.Item2).Window(size).Select(w => w.Last() - w.First());
            int count1 = source1.Select((a,i)=>Tuple.Create(a, i)).MaxBy(di => di.Item1)!.Item2 + num1;
            int count2 = source1.SelectWithIndex().MinBy(di => di.Item1)!.Item2 + num1;
            int num2 = num1 + size / 2;
            IEnumerable<double> source2 = source1.Window(size).Select(w => w.Last() - w.First());
            int index1 = source2.SelectWithIndex().Take(count1).MaxBy(di => di.Item1)!.Item2 + num2;
            int index2 = source2.SelectWithIndex().Skip(count2).MaxBy(di => di.Item1)!.Item2 + num2;
            return new Interval(values.ElementAt(index1).Item1.Time, values.ElementAt(index2).Item1.Time);
        }

        public static TimeSample FindNegative(
            IExamination data,
            int sensorIndex,
            Interval timeRange)
        {
            List<Tuple<TimeSample, double>> list = data.SampleValuesForSensorInTimeRange(timeRange, sensorIndex).ToList();
            if (list.Count < 3)
                throw new RegionFinderException("Not enough values in time range to determine peak change");
            return list.ElementAt(list.Select(v => v.Item2).Differences().Differences().SelectWithIndex()
                .MaxBy(di => di.Item1).Item2 + 1).Item1;
        }

        public void Track(DiagnosticsContext diagnostics, int sensorIndex, Interval timeRange)
        {
        }
    }
}