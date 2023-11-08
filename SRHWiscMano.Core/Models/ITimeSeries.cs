using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Models
{
    internal interface ITimeSeries
    {
        Instant Time { get;  }
        IReadOnlyList<double> Values { get; }
    }
}
