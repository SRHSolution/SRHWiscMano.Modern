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
        public static int SensorCount(this IExamData data)
        {
            return data.Samples.Count <= 0 ? 0 : data.Samples[0].Values.Count;
        }

        public static Range<int> SensorRange(this IExamData data)
        {
            return Range.Create(0, data.SensorCount());
        }

        public static Duration TickAmount(this IExamData data)
        {
            return data.Samples.Count <= 1
                ? Duration.FromMilliseconds(10L)
                : data.Samples[1].Time - data.Samples[0].Time;
        }

        public static Instant StartTime(this IExamData data)
        {
            return data.Samples.Count <= 0 ? SampleTime.Epoch : data.Samples[0].Time;
        }

        public static Instant EndTime(this IExamData data)
        {
            return data.Samples.Count <= 0 ? SampleTime.Epoch : data.Samples[data.Samples.Count - 1].Time;
        }

        public static Interval TotalTime(this IExamData data)
        {
            return new Interval(data.StartTime(), data.EndTime());
        }

        public static Duration TotalDuration(this IExamData data)
        {
            return data.EndTime() - data.StartTime();
        }
    }
}