namespace SRHWiscMano.Core.Models
{
    public static class Regions
    {
        public static readonly RegionType[] AllUsingMP = new RegionType[5]
        {
            RegionType.VP,
            RegionType.MP,
            RegionType.PreUES,
            RegionType.PostUES,
            RegionType.UES
        };

        public static readonly RegionType[] AllUsingTBAndHP = new RegionType[6]
        {
            RegionType.VP,
            RegionType.TB,
            RegionType.HP,
            RegionType.PreUES,
            RegionType.PostUES,
            RegionType.UES
        };

        public static RegionType[] All(RegionsVersion version)
        {
            if (version == RegionsVersion.UsesMP)
                return AllUsingMP;
            if (version == RegionsVersion.UsesTBAndHP)
                return AllUsingTBAndHP;
            throw new ArgumentException("Invalid regions version", nameof(version));
        }
    }
}