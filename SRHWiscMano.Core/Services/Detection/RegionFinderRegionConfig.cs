using NodaTime;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    public class RegionFinderRegionConfig
    {
        public RegionFinderRegionConfig(
            RegionType region,
            Duration initialWidth,
            IEdgeAlgorithm algorithm)
        {
            Region = region;
            InitialWidth = initialWidth;
            Algorithm = algorithm;
        }

        public RegionType Region { get; }

        public Duration InitialWidth { get; }

        public IEdgeAlgorithm Algorithm { get; }
    }
}