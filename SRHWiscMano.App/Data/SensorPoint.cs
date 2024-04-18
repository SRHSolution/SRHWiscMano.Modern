using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;

namespace SRHWiscMano.App.Data
{
    public class SensorPoint : IDataPointProvider, ICodeGenerating
    {
        public double Time { get; private set; }
        public double Sensor { get; private set; }
        public double Value { get; private set; }
        public double ValueScale { get; set; }



        public DataPoint GetDataPoint()
        {
            return new DataPoint(Time, Sensor + Value*ValueScale );
        }

        public string ToCode()
        {
            return CodeGenerator.FormatConstructor(this.GetType(), "Time : {0}\nSensor : {1}\nValue : {2}", this.Time, this.Sensor, this.Value);
        }
    }
}
