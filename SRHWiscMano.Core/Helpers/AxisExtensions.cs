using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Axes;

namespace SRHWiscMano.Core.Helpers
{
    public static class AxisExtensions
    {
        public static double GetWidth(this Axis axis)
        {
            var width = axis.Maximum - axis.Minimum + 1;
            return width;
        }

        public static double GetCenter(this Axis axis)
        {
            return GetWidth(axis) / 2;
        }
    }
}
