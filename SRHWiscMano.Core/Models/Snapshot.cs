using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public class Snapshot : ISnapshot
    {
        public Snapshot(
            string id,
            ITimeSeriesData data,
            string text,
            Instant time,
            Range<int> sensorRange,
            int? vpUpperBound,
            int? uesLowerBound,
            bool isSelected,
            bool normalEligible,
            ISnapshotLabels labels,
            IReadOnlyList<IRegion> regions,
            RegionsVersion regionsVersion)
        {
            Id = id;
            Data = data;
            Text = text;
            Time = time;
            SensorRange = sensorRange;
            VPUpperBound = vpUpperBound;
            UesLowerBound = uesLowerBound;
            IsSelected = isSelected;
            NormalEligible = normalEligible;
            Labels = labels;
            Regions = regions;
            RegionsVersion = regionsVersion;
        }

        public string Id { get; }
        public ITimeSeriesData Data { get; }
        public string Text { get; }
        public Instant Time { get; }
        public Range<int> SensorRange { get; }
        public int? VPUpperBound { get; }
        public int? UesLowerBound { get; }
        public bool IsSelected { get; }
        public bool NormalEligible { get; }
        public ISnapshotLabels Labels { get; }
        public IReadOnlyList<IRegion> Regions { get; }
        public RegionsVersion RegionsVersion { get; }
    }
}
