using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class PressureAtVPMaxCalculator
    {
        public static IEnumerable<double> Calc(ITimeFrame tFrame)
        {
            Instant time = tFrame.GetRegion(RegionType.VP).FocalPoint.Time;
            TimeSample sample = tFrame.ExamData.Samples.SamplesAtTime(time);
            return tFrame.InterpolatedValuesAtSample(sample);
        }
    }
}