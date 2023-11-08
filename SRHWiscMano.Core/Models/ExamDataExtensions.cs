using NodaTime;
using SRHWiscMano.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Models
{
    public static class ExamDataExtensions
    {
        public static int SensorCount(this ITimeSeriesData data)
        {
            return data.Samples.Count <= 0 ? 0 : data.Samples[0].Values.Count;
        }

        public static Range<int> SensorRange(this ITimeSeriesData data)
        {
            return Range.Create(0, data.SensorCount());
        }

        public static Duration TickAmount(this ITimeSeriesData data)
        {
            return data.Samples.Count <= 1
                ? Duration.FromMilliseconds(10L)
                : data.Samples[1].Time - data.Samples[0].Time;
        }

        public static Instant StartTime(this ITimeSeriesData data)
        {
            return data.Samples.Count <= 0 ? SampleTime.Epoch : data.Samples[0].Time;
        }

        public static Instant EndTime(this ITimeSeriesData data)
        {
            return data.Samples.Count <= 0 ? SampleTime.Epoch : data.Samples[data.Samples.Count - 1].Time;
        }

        public static Interval TotalTime(this ITimeSeriesData data)
        {
            return new Interval(data.StartTime(), data.EndTime());
        }

        public static Duration TotalDuration(this ITimeSeriesData data)
        {
            return data.EndTime() - data.StartTime();
        }
    }
}