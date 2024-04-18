using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Services.Detection
{
    internal static class BackgroundWindows
    {
        public static IEnumerable<Interval> Default(
            Interval testWindow,
            Duration windowWidth,
            Duration inset,
            Interval bounds)
        {
            return HalfBeforeHalfAfter(testWindow, windowWidth, inset, bounds);
        }

        private static IEnumerable<Interval> DirectlyAfter(
            Interval testWindow,
            Duration windowWidth,
            Interval bounds)
        {
            return new Interval[1]
            {
                new Interval(testWindow.End, testWindow.End + windowWidth).Clip(bounds)
            };
        }

        private static IEnumerable<Interval> HalfBeforeHalfAfter(
            Interval testWindow,
            Duration windowWidth,
            Duration inset,
            Interval bounds)
        {
            Interval interval = new Interval(testWindow.End - inset, testWindow.End + windowWidth / 2L - inset);
            return new Interval[2]
            {
                new Interval(testWindow.Start - windowWidth / 2L + inset, testWindow.Start + inset).Clip(bounds),
                interval.Clip(bounds)
            };
        }
    }
}