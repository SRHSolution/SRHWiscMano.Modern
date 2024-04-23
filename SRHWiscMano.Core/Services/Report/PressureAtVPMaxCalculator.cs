using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class PressureAtVPMaxCalculator
    {
        public static IEnumerable<double> Calc(ITimeFrame snapshot)
        {
            Instant time = snapshot.GetRegion(RegionType.VP).FocalPoint.Time;
            TimeSample sample = snapshot.ExamData.Samples.SamplesAtTime(time);
            return snapshot.InterpolatedValuesAtSample(sample);
        }
    }
}