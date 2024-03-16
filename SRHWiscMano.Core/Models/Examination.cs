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