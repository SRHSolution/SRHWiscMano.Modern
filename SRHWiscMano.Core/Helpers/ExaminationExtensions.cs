using NodaTime;
using SRHWiscMano.Core.Models;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Helpers
{
    public static class ExaminationExtensions
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
            return data.Samples.Count <= 0 ? InstantExtensions.Epoch : data.Samples[0].Time;
        }

        public static Instant EndTime(this IExamination data)
        {
            return data.Samples.Count <= 0 ? InstantExtensions.Epoch : data.Samples[data.Samples.Count - 1].Time;
        }

        public static Interval TotalTime(this IExamination data)
        {
            return new Interval(data.StartTime(), data.EndTime());
        }

        public static Duration TotalDuration(this IExamination data)
        {
            return data.EndTime() - data.StartTime();
        }

        public static int SampleIndexFromTime(this IExamination data, Instant time)
        {
            time = RoundToNearestTick(time, data.TickAmount());
            int num = (int)((time.ToUnixTimeTicks() - data.StartTime().ToUnixTimeTicks()) / data.TickAmount().TotalTicks);
            if (num < 0 || num >= data.Samples.Count)
                throw new ArgumentOutOfRangeException(nameof(time), num,
                    "The time specified does not fall within the range of the data.");
            TimeSample sample = data.Samples[num];
            if (sample.Time != time)
                throw new ArgumentException(
                    string.Format("Time/index mismatch.  Did the tick amount change in the data? (sample={0} time={1})",
                        sample.Time.ToMillisecondsFromEpoch(), time.ToMillisecondsFromEpoch()), nameof(time));
            return num;
        }

        private static Instant RoundToNearestTick(Instant time, Duration tickAmount)
        {
            long num = (long)(time.ToUnixTimeTicks() % tickAmount.TotalTicks);
            if (num == 0L)
                return time;

            return num < tickAmount.TotalTicks / 2L
                ? Instant.FromUnixTimeTicks(time.ToUnixTimeTicks() - num)
                : Instant.FromUnixTimeTicks(time.ToUnixTimeTicks() + ((long)tickAmount.TotalTicks - num));
        }
    }
}