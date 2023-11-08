using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Models
{
    internal class TimeSeriesData : ITimeSeriesData
    {
        public IReadOnlyList<Sample> Samples { get; }
        public IReadOnlyList<Note> Notes { get; }

        public TimeSeriesData(IList<Sample> samples, IList<Note> notes)
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
