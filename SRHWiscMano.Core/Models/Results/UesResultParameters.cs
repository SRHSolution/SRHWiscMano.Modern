using SRHWiscMano.Core.Services.Report;

namespace SRHWiscMano.Core.Models.Results
{
    public class UesResultParameters<T> : ResultParameters<T>
    {
        public T NadirDuration { get; set; }
        public T NadirDurationMid { get; set; }

        public T MinimumPressure { get; set; }

        public UesNadirCalculator.SensorSample MinimumSample { get; set; }
    }
}