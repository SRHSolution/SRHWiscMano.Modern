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

        /// <summary>
        /// Plot 을 위한 ExamData에서 Scale 이 적용된 데이터
        /// </summary>
        [Obsolete]
        double[,] PlotData { get; }

        void UpdateTime(Instant newTime);
    }
}