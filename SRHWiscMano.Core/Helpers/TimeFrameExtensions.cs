using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Helpers
{
    public static class TimeFrameExtensions
    {
        

        public static int SensorCount(this ITimeFrame data)
        {
            return data.FrameSamples.Count <= 0 ? 0 : data.FrameSamples[0].Values.Count;
        }

        public static Range<int> SensorRange(this ITimeFrame data)
        {
            return Range.Create(0, data.SensorCount());
        }

        public static Duration TickAmount(this ITimeFrame data)
        {
            return data.FrameSamples.Count <= 1
                ? Duration.FromMilliseconds(10L)
                : data.FrameSamples[1].Time - data.FrameSamples[0].Time;
        }

        public static Instant StartTime(this ITimeFrame data)
        {
            return data.FrameSamples.Count <= 0 ? InstantExtensions.Epoch : data.FrameSamples[0].Time;
        }

        public static Instant EndTime(this ITimeFrame data)
        {
            return data.FrameSamples.Count <= 0 ? InstantExtensions.Epoch : data.FrameSamples[data.FrameSamples.Count - 1].Time;
        }

        public static Interval TimeRange(this ITimeFrame data)
        {
            return new Interval(data.StartTime(), data.EndTime());
        }

        public static Duration TotalDuration(this ITimeFrame data)
        {
            return data.EndTime() - data.StartTime();
        }
    }
}
