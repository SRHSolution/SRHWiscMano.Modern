using NodaTime;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    public interface IDetectionDiagnostics
    {
        void ExpandTimeRange(Interval timeRange);

        void AddMarker(
            RegionType region,
            Instant time,
            int sensorIndex,
            DetectionMarkerType markerType);

        void SetThreshold(RegionType region, int sensorIndex, double threshold);
    }
}