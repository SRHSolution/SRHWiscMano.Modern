using NodaTime;
using SRHWiscMano.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class MinPressureCalculator
    {
        public static IEnumerable<double> Calc(ITimeFrame tFrame)
        {
            var orgRes = RegionInterpolation.Sections()
                .Select(p => InterpolatedMinsForRegion(tFrame, p.Item1)).SelectMany(m => m);

            return orgRes;
        }

        /// <summary>
        /// 입력된 Region 의 센서 영역에 대해서 각 센서의 SmoothMin 값을 계산하고 센서 값들 사이를 Interpolation 한 결과로 반환한다.
        /// </summary>
        /// <param name="tFrame"></param>
        /// <param name="regionType"></param>
        /// <returns></returns>
        private static IEnumerable<double> InterpolatedMinsForRegion(ITimeFrame tFrame, RegionType regionType)
        {
            var resArr = tFrame.GetRegion(regionType).SensorRange.AsEnumerable().Select(i => new
                {
                    i,
                    tr = TimeRangeForRegionsOnSensor(tFrame, i)
                }).Where(_param1 => _param1.tr.HasValue)
                .Select(_param1 => tFrame.ExamData.Samples.SamplesInTimeRange(_param1.tr.Value).SmoothMin(_param1.i))
                .ToArray();

            return resArr;
        }

        /// <summary>
        /// 센서 값이 region 에 걸쳐있는 시간간격 interval을 찾는다
        /// </summary>
        /// <param name="tFrame"></param>
        /// <param name="sensor"></param>
        /// <returns></returns>
        private static Interval? TimeRangeForRegionsOnSensor(ITimeFrame tFrame, int sensor)
        {
            IEnumerable<Interval> source = tFrame.Regions.Items.Where(r => r.SensorRange.Contains(sensor))
                .Select(r => r.TimeRange);
            return !source.Any() ? new Interval?() : source.Aggregate((p, c) => p.Union(c));
        }
    }
}