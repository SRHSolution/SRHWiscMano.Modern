using System.IO;
using System.Windows.Media;
using NodaTime;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services.Detection;
using SRHWiscMano.Core.Services.Report;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Services.Diagnostics
{
    internal class SensorChart : Chart
    {
        private readonly ISensorChartParams owner;
        private readonly int sensorIndex;

        public SensorChart(ISensorChartParams owner, RegionType region, int sensorIndex)
            : base(region + "-sensor-" + (sensorIndex + 1), "Sensor " + (sensorIndex + 1), "ms", "mmHg", 20)
        {
            this.owner = owner;
            this.sensorIndex = sensorIndex;
            Markings = new List<Tuple<Instant, DetectionMarkerType>>();
        }

        public List<Tuple<Instant, DetectionMarkerType>> Markings { get; private set; }

        public double Threshold { get; set; }

        public override void WriteHtml(TextWriter writer)
        {
            writer.WriteLine("\t\t<div class=\"row\">");
            writer.WriteLine("\t\t\t<div class=\"col-xs-1 col-sm-1\">");
            writer.WriteLine("\t\t\t\t<h3>{0}</h3>", sensorIndex);
            writer.WriteLine("\t\t\t</div>");
            writer.WriteLine("\t\t\t<div class=\"col-xs-11 col-sm-11\">");
            writer.WriteLine("\t\t\t\t<div class=\"chart-box\">");
            writer.WriteLine("\t\t\t\t\t<div id=\"{0}\" class=\"chart\"></div>", Id);
            writer.WriteLine("\t\t\t\t</div>");
            writer.WriteLine("\t\t\t</div>");
            writer.WriteLine("\t\t</div>");
        }

        public override string RenderJS()
        {
            return null;
            // Instant center = owner.TimeRange.Center();
            // return Flot.Plot(Id, new string[1]
            //     {
            //         Flot.Series(Colors.Black, 1.0,
            //             owner.TimeFrame.ExamData.Samples.SamplesInTimeRange(owner.TimeRange).Select(s =>
            //                 Tuple.Create((s.Time - center).ToTimeSpan().TotalSeconds, s.Values[sensorIndex])))
            //     },
            //     "{" + string.Join(",", Flot.YAxis(Range.Create(-20.0, 100.0)),
            //         "\"grid\":{\"markings\":[" + string.Join(",",
            //             Markings.Select(m => FormatMarking(m, center)).Concat(FormatThreshold(Threshold))) + "]}") +
            //     "}");
        }

        private static string FormatMarking(Tuple<Instant, DetectionMarkerType> marking, Instant epoch)
        {
            return "{\"xaxis\":{" +
                   string.Format("\"from\":{0},\"to\":{0}", (marking.Item1 - epoch).ToTimeSpan().TotalSeconds) + "}," +
                   Flot.Color(ColorForMarker(marking.Item2)) + "}";
        }

        private static string FormatThreshold(double threshold)
        {
            return "{\"yaxis\":{" + string.Format("\"from\":{0},\"to\":{0}", threshold) +
                   "},\"color\":\"rgba(225,127,127,0.3)\"}";
        }

        private static Color ColorForMarker(DetectionMarkerType markerType)
        {
            switch (markerType)
            {
                case DetectionMarkerType.Edge:
                    return Colors.Red;
                case DetectionMarkerType.TestWindow:
                    return Colors.Orange;
                case DetectionMarkerType.BackgroundWindow:
                    return Colors.Blue;
                case DetectionMarkerType.FirstPeak:
                    return Colors.Cyan;
                default:
                    return Colors.DarkGray;
            }
        }
    }
}