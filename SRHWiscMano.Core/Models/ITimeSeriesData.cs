using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Models
{
    public interface ITimeSeriesData
    {
        IReadOnlyList<Sample> Samples { get; }
        IReadOnlyList<Note> Notes { get; }
    }
}
