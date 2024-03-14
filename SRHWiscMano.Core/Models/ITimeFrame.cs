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

        /// <summary>
        /// 분석을 위해 입력된 원본 데이터
        /// </summary>
        double[,] ExamData { get; }

        /// <summary>
        /// Plot 을 위한 ExamData에서 Scale 이 적용된 데이터
        /// </summary>
        double[,] PlotData { get; }

        void UpdateTime(Instant newTime);
    }
}