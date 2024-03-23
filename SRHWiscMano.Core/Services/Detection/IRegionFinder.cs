using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    public interface IRegionFinder
    {
        Region Find(
            RegionType type,
            ITimeFrame state,
            SamplePoint click,
            RegionFinderConfig config,
            IDetectionDiagnostics diagnostics);
    }
}