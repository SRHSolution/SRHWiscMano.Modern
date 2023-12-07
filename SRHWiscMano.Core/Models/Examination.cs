using System.Collections.Immutable;
using System.Text;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public class Examination : IExamination
    {
        public IReadOnlyList<TimeSample> Samples { get; }
        public IReadOnlyList<FrameNote> Notes { get; }
        public int InterpolationScale { get; private set; }
        public double[,] PlotData { get; private set; }

        public Examination(IList<TimeSample> samples, IList<FrameNote> notes)
        {
            this.Samples = samples.ToImmutableList();
            this.Notes = notes.ToImmutableList();
        }

        public Task UpdatePlotData(int interpolateScale)
        {
            this.InterpolationScale = interpolateScale;
            return Task.Run(() => { UpdatePlotDataImp(interpolateScale); });
        }

        private void UpdatePlotDataImp(int interpolateScale)
        {
            var sensorCount = (int)(this.SensorCount() * interpolateScale);
            var frameCount = Samples.Count;
            
            PlotData = new double[frameCount, sensorCount];

            for (int i = 0; i < frameCount; i++)
            {
                var scaledSensorValues =
                    Interpolators.LinearInterpolate(Samples[i].Values.ToArray(), sensorCount);

                for (int j = 0; j < sensorCount; j++)
                {
                    PlotData[i, j] = scaledSensorValues[sensorCount - 1 - j];
                }
            }
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