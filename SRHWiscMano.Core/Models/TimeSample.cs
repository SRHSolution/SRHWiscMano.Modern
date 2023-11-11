using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{

    public class TimeSample : ITimeSeries
    {
        private readonly Instant time;
        private readonly IReadOnlyList<double> values;

        public TimeSample(Instant time, IReadOnlyList<double> values)
        {
            this.time = time;
            this.values = values;
        }

        public Instant Time => time;

        public double TimeInSeconds => (time - InstantExtensions.Epoch).ToTimeSpan().TotalSeconds;

        public IReadOnlyList<double> Values => values;

        public int DataSize => Values.Count;
    }
}