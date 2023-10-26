using NodaTime;

namespace SRHWiscMano.Core.Models
{

    public class Sample : ITimeSeries
    {
        private readonly Instant time;
        private readonly IReadOnlyList<double> values;

        public Sample(Instant time, IReadOnlyList<double> values)
        {
            this.time = time;
            this.values = values;
        }

        public Instant Time => time;

        public double TimeInSeconds => (time - SampleTime.Epoch).ToTimeSpan().TotalSeconds;

        public IReadOnlyList<double> Values => values;

        public int DataSize => Values.Count;
    }
}