using System.Windows.Media;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Services.Report
{
    internal static class Flot
    {
        public static string Series(
            Color color,
            double lineWidth,
            IEnumerable<Tuple<double, double>> data)
        {
            string str1 = string.Format("rgb({0},{1},{2})", color.R, color.G, color.B);
            string str2 = string.Join(",", data.Select(p => string.Format("[{0},{1}]", p.Item1, p.Item2)));
            return "{\"color\":\"" + str1 + "\",\"lines\":{\"lineWidth\":" + lineWidth +
                   "},\"shadowSize\":0,\"data\":[" + str2 + "]}";
        }

        public static string Series(
            Color color,
            double lineWidth,
            Range<double> yaxisRange,
            IEnumerable<Tuple<double, double>> data)
        {
            return "{" + string.Join(",", Color(color), LineWidth(lineWidth), YAxis(yaxisRange), ShadowSize(0),
                Data(data)) + "}";
        }

        public static string Plot(string chartId, IEnumerable<string> series, string options)
        {
            string str = "[" + string.Join(",", series) + "]";
            if (options == null)
                options = "{}";
            return string.Format("$.plot($(\"#{0}\"), {1}, {2});", chartId, str, options);
        }

        public static string Color(Color color)
        {
            return string.Format("\"color\":\"rgb({0},{1},{2})\"", color.R, color.G, color.B);
        }

        public static string YAxis(Range<double> range)
        {
            return "\"yaxis\":{" + string.Format("\"min\":{0},\"max\":{1}", range.Start, range.End) + "}";
        }

        private static string LineWidth(double width)
        {
            return "\"lines\":{\"lineWidth\":" + width + "}";
        }

        private static string ShadowSize(int shadowSize)
        {
            return string.Format("\"shadowSize\":{0}", shadowSize);
        }

        private static string Data(IEnumerable<Tuple<double, double>> data)
        {
            return "\"data\":[" + string.Join(",", data.Select(p => string.Format("[{0},{1}]", p.Item1, p.Item2))) +
                   "]";
        }
    }
}