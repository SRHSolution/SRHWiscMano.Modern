using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface IRegion
    {
        ITimeFrame Window { get; }

        Interval TimeRange { get; }

        Range<int> SensorRange { get; }

        IList<double> MaxValues { get; }
        IList<double> AvgValues { get; }

        RegionType Type { get; }

        /// <summary>
        /// 사용자가 Region을 찾기 위해 임의로 click 한 위치 Point
        /// </summary>
        SamplePoint ClickPoint { get; }

        /// <summary>
        /// Region 에서 계산된 최대 값을 갖는 Point
        /// </summary>
        SamplePoint FocalPoint { get; }
    }
}