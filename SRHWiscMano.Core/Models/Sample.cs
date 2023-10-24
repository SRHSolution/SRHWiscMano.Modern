using NodaTime;

namespace SRHWiscMano.Core.Models
{
    public class Sample
    {
        private readonly IReadOnlyList<double> impedances;
        private readonly Instant time;
        private readonly IReadOnlyList<double> values;

        public Sample(Instant time, IReadOnlyList<double> values, IReadOnlyList<double> impedances)
        {
            this.time = time;
            this.values = values;
            this.impedances = impedances;
        }

        public Instant Time => time;

        public double TimeInSeconds => (time - SampleTime.Epoch).ToTimeSpan().TotalSeconds;

        public IReadOnlyList<double> Values => values;

        public IReadOnlyList<double> Impedances => impedances;
    }
}