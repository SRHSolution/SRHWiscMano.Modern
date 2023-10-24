using NodaTime;
using SRHWiscMano.Core.Helpers;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Models
{
    public static class SampleTime
    {
        public static readonly Instant Epoch = new Instant();
        public static readonly Interval EmptyInterval = new Interval(Epoch, Epoch);

        public static Instant InstantFromMilliseconds(long milliseconds)
        {
            return Epoch + Duration.FromMilliseconds(milliseconds);
        }

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
    }
}