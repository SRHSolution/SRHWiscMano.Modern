using DynamicData;
using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface ITimeFrame : ICloneable
    {
        IExamination ExamData { get; }
        int Id { get; }
        string Text { get; set; }
        Instant Time { get; }
        double TimeDuration { get; }

        double MinSensorBound { get; }
        double MaxSensorBound { get; }
        ISourceList<IRegion> Regions { get; }

        public IReadOnlyList<TimeSample> FrameSamples { get; }
        public IReadOnlyList<TimeSample> IntpFrameSamples { get; }

        void UpdateTime(Instant newTime);

        void UpdateSensorBounds(double minBound, double maxBound);
        IRegion GetRegion(RegionType regionType);
    }
}