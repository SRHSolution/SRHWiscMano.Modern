using System.Xml.Linq;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Detection
{
    public class RegionFinderConfig
    {
        public static readonly RegionFinderConfig Default = new RegionFinderConfig(null);
        private Dictionary<RegionType, RegionFinderRegionConfig> regionConfigs;

        public RegionFinderConfig(
            Dictionary<RegionType, RegionFinderRegionConfig> regionConfigs)
        {
            this.regionConfigs = regionConfigs;
        }

       
        public RegionFinderRegionConfig GetConfigForRegion(RegionType region)
        {
            Dictionary<RegionType, RegionFinderRegionConfig> regionConfigs = this.regionConfigs;
            return DefaultConfigForRegion(region);
        }

        

        private static RegionFinderRegionConfig DefaultConfigForRegion(
            RegionType region)
        {
            switch (region)
            {
                case RegionType.VP:
                    return new RegionFinderRegionConfig(region, Duration.FromMilliseconds(3000L),
                        new RangeAboveBaseline(Duration.FromMilliseconds(4000L), Duration.Zero));
                case RegionType.TB:
                    return new RegionFinderRegionConfig(region, Duration.FromMilliseconds(3000L),
                        new RangeAboveBaseline(Duration.FromMilliseconds(4000L), Duration.Zero));
                case RegionType.HP:
                    return new RegionFinderRegionConfig(region, Duration.FromMilliseconds(1000L),
                        new RangeAboveBaseline(Duration.FromMilliseconds(4000L), Duration.Zero));
                default:
                    return null;
            }
        }
    }
}