using System.Collections.Immutable;
using System.Text;
using DynamicData;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public class Examination : IExamination
    {
        /// <summary>
        /// TimeSample 전체를 포함하는 통합데이터
        /// </summary>
        public IReadOnlyList<TimeSample> Samples { get; }
        public IReadOnlyList<FrameNote> Notes { get; }
        public int InterpolationScale { get; private set; }
        public double[,] PlotData { get; private set; }

        public Examination(IList<TimeSample> samples, IList<FrameNote> notes)
        {
            this.Samples = samples.ToImmutableList();
            this.Notes = notes.ToImmutableList();
        }

        public Task UpdatePlotData(int interpolateScale = 1)
        {
            this.InterpolationScale = interpolateScale;
            return Task.Run(() => { UpdatePlotDataImp(interpolateScale); });
        }

        
        private void UpdatePlotDataImp(int interpolateScale)
        {
            var sensorCount = (int)(this.SensorCount() * interpolateScale);
            var frameCount = Samples.Count;
            
            PlotData = new double[frameCount, sensorCount];
            Parallel.For(0, frameCount, i =>
            {
                double[] scaledSensorValues = {};
                if (interpolateScale != 1)
                {
                    scaledSensorValues =
                        Interpolators.LinearInterpolate(Samples[i].Values.ToArray(), sensorCount);
                }
                else
                {
                    scaledSensorValues = Samples[i].Values.ToArray();
                }
            
                for (int j = 0; j < sensorCount; j++)
                {
                    PlotData[i, j] = scaledSensorValues[sensorCount - 1 - j];
                }
            });
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Sample Range : {this.SensorRange()}");
            sb.AppendLine($"Sample Count : {this.SensorCount()}");
            sb.AppendLine($"Sample Duration: {this.TotalDuration()} ms");
            sb.AppendLine($"Notes Counts : {Notes.Count}");

            return sb.ToString();
        }
    }
}