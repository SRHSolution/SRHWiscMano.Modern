using NodaTime;

namespace SRHWiscMano.Core.Models
{
    public class DataPoint
    {
        public DataPoint(Instant time, int sensor)
        {
            Time = time;
            Sensor = sensor;
        }

        public Instant Time { get; }

        public int Sensor { get; }

        public static DataPoint TryLoad(long? time, int? sensor)
        {
            return !time.HasValue || !sensor.HasValue
                ? null
                : new DataPoint(SampleTime.InstantFromMilliseconds(time.Value), sensor.Value);
        }
    }
}