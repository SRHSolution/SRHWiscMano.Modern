using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface ITimeFrame : ICloneable
    {
        int Id { get; }
        string Text { get; set; }
        Instant Time { get; }
        double TimeDuration { get; }

        public IReadOnlyList<TimeSample> FrameSamples { get; }
        public IReadOnlyList<TimeSample> IntpFrameSamples { get; }

        void UpdateTime(Instant newTime);
    }
}