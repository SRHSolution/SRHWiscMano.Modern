using OxyPlot;

namespace SRHWiscMano.Core.Models
{
    public class SensorPoint : IDataPointProvider, ICodeGenerating
    {
        public double Time { get; set; }
        public double Sensor { get; set; }
        public double Value { get; set; }
        public double ValueScale { get; set; }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(Time, Sensor + Value*ValueScale );
        }

        public string ToCode()
        {
            // return CodeGenerator.FormatConstructor(this.GetType(), "Time : {0}\nSensor : {1}\nValue : {2}", this.Time, this.Sensor, this.Value);
            return CodeGenerator.FormatConstructor(this.GetType(), "{0},{1}", this.Time, this.Sensor);//"Time : {0}\nSensor : {1}\nValue : {2}", this.Time, this.Sensor, this.Value);
        }
    }
}
