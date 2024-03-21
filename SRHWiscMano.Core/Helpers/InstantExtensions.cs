using NodaTime;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Helpers
{
    /// <summary>
    /// Sample 시간을 통합하여 Duration과 Instant로 변환하여 관리한다.
    /// Tick 은 100 ns의 값을 갖는다.  사용되는 센서데이터는 10msec 이며, 각 시간 단위별로 값의 크기는 아래와 같다
    /// - 10 ms
    /// - 10,000 us
    /// - 10,000,000 ns
    /// - 100,000 Tick
    /// Interval : 1970-01-01T00:00:00Z
    /// Duration : 0:00:10:17.86
    /// </summary>
    public static class InstantExtensions
    {
        public static readonly Instant Epoch = new();
        public static readonly Interval EmptyInterval = new(Epoch, Epoch);

        public static long ToMillisecondsFromEpoch(this Instant time)
        {
            return (time - Epoch).ToMilliseconds();
        }

        public static long ToMilliseconds(this Duration duration)
        {
            return (long)duration.ToTimeSpan().TotalMilliseconds;
        }

        public static Interval Map(this Interval interval, Func<Instant, Instant> converter)
        {
            return new Interval(converter(interval.Start), converter(interval.End));
        }

        public static Range<double> Map(this Interval interval, Func<Instant, double> converter)
        {
            return Range.Create(converter(interval.Start), converter(interval.End));
        }

        public static Interval Union(this Interval a, Interval b)
        {
            return new Interval(Instant.Min(a.Start, b.Start), Instant.Max(a.End, b.End));
        }

        public static double PercentOf(this Interval interval, Instant value)
        {
            Duration duration = value - interval.Start;
            double ticks1 = duration.TotalTicks;
            duration = interval.Duration;
            double ticks2 = duration.TotalTicks;
            return ticks1 / ticks2;
        }

        /// <summary>
        /// 입력된 시간을 중심으로 interval를 생성하되, bounds 영역밖은 잘라낸다
        /// </summary>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Interval AtCenterOfDuration(
            this Instant time,
            Duration duration,
            Interval bounds)
        {
            Duration duration1 = duration / 2L;
            return new Interval(time - duration1, time + duration1).Clip(bounds);
        }

        /// <summary>
        /// 입력된 시간을 시작으로 duration 을 갖는 interval 를 계산하며, bounds의 end 영역을 clip 한다 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Interval AtStartOfDuration(
            this Instant time,
            Duration duration,
            Interval bounds)
        {
            Instant end = Instant.Min(time + duration, bounds.End);
            return new Interval(time, end);
        }

        /// <summary>
        /// interval 의 center를 계산한다
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static Instant Center(this Interval interval)
        {
            Duration duration = interval.Duration / 2L;
            return interval.Start + duration;
        }

        /// <summary>
        /// interval 이 bound 를 넘어갈 경우 min,max로 자른다
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Interval Clip(this Interval interval, Interval bounds)
        {
            return new Interval(Instant.Max(interval.Start, bounds.Start), Instant.Min(interval.End, bounds.End));
        }

        public static Duration AbsoluteValue(this Duration duration)
        {
            return !(duration < Duration.Zero) ? duration : -duration;
        }
    }
}