using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public class TimeFrame : ITimeFrame
    {
        public TimeFrame(
            string id,
            IExamination data,
            string text,
            Instant time,
            Range<int> sensorRange,
            int? vpUpperBound,
            int? uesLowerBound,
            bool isSelected,
            bool normalEligible,
            ITimeFrameLabels labels,
            IReadOnlyList<IRegion> regions,
            RegionsVersionType regionsVersionType)
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
            RegionsVersionType = regionsVersionType;
        }

        public string Id { get; }
        public IExamination Data { get; }
        public string Text { get; }
        public Instant Time { get; }
        public Range<int> SensorRange { get; }
        public int? VPUpperBound { get; }
        public int? UesLowerBound { get; }
        public bool IsSelected { get; }
        public bool NormalEligible { get; }
        public ITimeFrameLabels Labels { get; }
        public IReadOnlyList<IRegion> Regions { get; }
        public RegionsVersionType RegionsVersionType { get; }
    }
}
