namespace SRHWiscMano.Core.Models.Results
{
    public class UesResultParameters<T> : ResultParameters<T>
    {
        public T NadirDuration { get; set; }

        public T MinimumPressure { get; set; }
    }
}