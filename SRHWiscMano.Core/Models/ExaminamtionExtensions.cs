using NodaTime;
using SRHWiscMano.Core.Helpers;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Models
{
    public static class ExaminamtionExtensions
    {
        public static int SensorCount(this IExamination data)
        {
            return data.Samples.Count <= 0 ? 0 : data.Samples[0].Values.Count;
        }

        public static Range<int> SensorRange(this IExamination data)
        {
            return Range.Create(0, data.SensorCount());
        }

        public static Duration TickAmount(this IExamination data)
        {
            return data.Samples.Count <= 1
                ? Duration.FromMilliseconds(10L)
                : data.Samples[1].Time - data.Samples[0].Time;
        }

        public static Instant StartTime(this IExamination data)
        {
            return data.Samples.Count <= 0 ? SampleTime.Epoch : data.Samples[0].Time;
        }

        public static Instant EndTime(this IExamination data)
        {
            return data.Samples.Count <= 0 ? SampleTime.Epoch : data.Samples[data.Samples.Count - 1].Time;
        }

        public static Interval TotalTime(this IExamination data)
        {
            return new Interval(data.StartTime(), data.EndTime());
        }

        public static Duration TotalDuration(this IExamination data)
        {
            return data.EndTime() - data.StartTime();
        }
    }
}