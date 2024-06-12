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
            var samples = region.SamplesInRegion();
            List<double> myVals = new List<double>();
            foreach (var ss in samples)
            {
                myVals.Add(ss.Values[region.ClickPoint.Sensor]);
            }
            return region.SamplesInRegion().MaxBy(s => s.Values.Where((v, i) => i == region.ClickPoint.Sensor).Max());
        }


        public static TimeSample? SampleWithMinimumOnClickedSensor(this IRegion region)
        {
            var samples = region.SamplesInRegion();
            List<double> myVals = new List<double>();
            foreach (var ss in samples)
            {
                myVals.Add(ss.Values[region.ClickPoint.Sensor]);
            }
            return region.SamplesInRegion().MinBy(s => s.Values.Where((v, i) => i == region.ClickPoint.Sensor).Min());
        }
    }
}