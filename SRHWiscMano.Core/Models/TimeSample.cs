using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{

    public class TimeSample : ITimeSeries
    {
        /// <summary>
        /// Sensor 데이터가 기록된 시간
        /// </summary>
        private readonly Instant time;
        
        /// <summary>
        /// Instant 시간에 갖는 Sensor의 데이터를 list로 갖는다
        /// </summary>
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