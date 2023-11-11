using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    /// <summary>
    /// TimeSample 에서 특정 센서의 값
    /// </summary>
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
                : new DataPoint(InstantUtils.InstantFromMilliseconds(time.Value), sensor.Value);
        }
    }
}