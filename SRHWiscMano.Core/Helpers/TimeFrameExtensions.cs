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
            return Range.Create((int)data.MinSensorBound, (int)data.MaxSensorBound);//data.SensorCount());
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


        public static IRegion GetRegion(this ITimeFrame tFrame, RegionType type)
        {
            return tFrame.Regions.Items.FirstOrDefault(r => r.Type == type) ??
                   throw new ArgumentException($"RegionType not found {type}", nameof(type));
        }

        public static bool AllRegionsAreDefined(this ITimeFrame tFrame)
        {
            RegionType[] second = Regions.All(RegionsVersionType.UsesTBAndHP);
            return tFrame.Regions.Items.Select(r => r.Type).Intersect(second).Count() == second.Length;
        }
    }
}
