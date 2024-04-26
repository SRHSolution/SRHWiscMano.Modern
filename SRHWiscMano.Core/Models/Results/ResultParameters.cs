namespace SRHWiscMano.Core.Models.Results
{
    public class ResultParameters<T>
    {
        /// <summary>
        /// 압력을 측정된 영역이 시간
        /// </summary>
        public T Duration { get; set; }

        /// <summary>
        /// 최대 압력값
        /// </summary>
        public T MaximumPressure { get; set; }

        /// <summary>
        /// 센서가 최대 값을 내는 시점까지의 적분된 압력값
        /// </summary>
        public T TotalPressureFromMaxSensor { get; set; }

        /// <summary>
        /// 전체 영역에 대한 적분된 압력값
        /// </summary>
        public T TotalVolumePressure { get; set; }
    }
}