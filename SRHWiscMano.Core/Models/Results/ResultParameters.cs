namespace SRHWiscMano.Core.Models.Results
{
    public class ResultParameters<T>
    {
        public T Duration { get; set; }

        public T MaximumPressure { get; set; }

        public T TotalPressureFromMaxSensor { get; set; }

        public T TotalVolumePressure { get; set; }
    }
}