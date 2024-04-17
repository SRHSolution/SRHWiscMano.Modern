using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Helpers
{
    public static class RegionExtensions
    {
        public static IEnumerable<TimeSample> SamplesInRegion(this IRegion region)
        {
            return region.Window.FrameSamples.SamplesInTimeRange(region.TimeRange);
        }

        /// <summary>
        /// 클릭한 위치의 region 내의 센서 값들 중에 최대 값을 얻는다
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public static TimeSample? SampleWithMaximumOnClickedSensor(this IRegion region)
        {
            return region.SamplesInRegion().MaxBy(s => s.Values.Where((v, i) => i == region.ClickPoint.Sensor).Max());
        }
    }
}