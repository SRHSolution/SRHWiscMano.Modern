using NodaTime;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    public class DiagnosticsContext
    {
        public DiagnosticsContext(IDetectionDiagnostics diagnostics, RegionType region)
        {
            Diagnostics = diagnostics;
            Region = region;
        }

        public IDetectionDiagnostics Diagnostics { get; }

        public RegionType Region { get; }

        public void ExpandTimeRange(Interval timeRange)
        {
            Diagnostics?.ExpandTimeRange(timeRange);
        }

        public void AddMarker(Instant time, int sensorIndex, DetectionMarkerType markerType)
        {
            Diagnostics?.AddMarker(Region, time, sensorIndex, markerType);
        }

        public void SetThreshold(int sensorIndex, double threshold)
        {
            Diagnostics?.SetThreshold(Region, sensorIndex, threshold);
        }

        public void TrackEdges(Interval edges, int sensorIndex, DetectionMarkerType markerType)
        {
            if (Diagnostics == null)
                return;
            Diagnostics.ExpandTimeRange(edges);
            Diagnostics.AddMarker(Region, edges.Start, sensorIndex, markerType);
            Diagnostics.AddMarker(Region, edges.End, sensorIndex, markerType);
        }
    }
}