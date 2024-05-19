using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using Range = SRHWiscMano.Core.Helpers.Range;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class PressureAtTBMaxCalculator
    {
        public static IEnumerable<double> Calc(ITimeFrame timeFrame)
        {
            RegionType type = RegionType.TB;
            IRegion mp = timeFrame.GetRegion(type);
            
            //1.전체 Sample 에서 TB 영역의 SensorRange & TimeRange의 Sample을 추출한다.
            //2.최대 value 값을 갖는 sample을 찾고 이 sample을 interpolation 한 값을 찾는다.
            TimeSample sample =
                (mp.SensorRange.Span() > 2
                    ? Range.Create(mp.SensorRange.Start, mp.SensorRange.Start + 2)
                    : mp.SensorRange).AsEnumerable()
                .Select(i => TimeSampleExtensions.SampleValuesForSensorInTimeRange(timeFrame.ExamData,mp.TimeRange, i)).SelectMany(t => t)
                .MaxBy(t => t.Item2).Item1;
            return timeFrame.InterpolatedValuesAtSample(sample);    // examData 에서 갖고 있는 Intpr value를 찾으면 된다.
        }
    }
}