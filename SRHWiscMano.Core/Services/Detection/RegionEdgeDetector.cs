using NLog;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    internal class RegionEdgeDetector
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Interval FindWithTestWindow(
            IEdgeAlgorithm algorithm,
            IExamination data,
            Range<int> betweenSensors,
            Interval testWindow,
            RegionType region,
            DiagnosticsContext diag)
        {
            List<Interval> list = FindValidEdges(algorithm, data, betweenSensors, testWindow, diag).ToList();
            return list.Any()
                ? TimeRangeFromEdges(list)
                : throw new RegionFinderException(
                    string.Format("No valid edges found in region {0}", region.ToString()));
        }

        internal static IEnumerable<Interval> FindValidEdges(
            IEdgeAlgorithm algorithm,
            IExamination data,
            Range<int> betweenSensors,
            Interval testWindow,
            DiagnosticsContext diag)
        {
            return betweenSensors.AsEnumerable()
                .Select(sensorIndex => algorithm.FindOnsetEdges(data, sensorIndex, testWindow, diag))
                .Where(e => e.HasValue).Select(e => e.Value);
        }

        internal static Interval TimeRangeFromEdges(IEnumerable<Interval> edgesPerSensor)
        {
            return new Interval(edgesPerSensor.Min(e => e.Start), edgesPerSensor.Max(e => e.End));
        }
    }
}