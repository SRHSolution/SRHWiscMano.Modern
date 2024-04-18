using NodaTime;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    public interface IEdgeAlgorithm
    {
        Interval? FindOnsetEdges(
            IExamination data,
            int sensorIndex,
            Interval timeRange,
            DiagnosticsContext diagnostics);
    }
}