using System.Diagnostics;
using System.IO;
using System.Text;
using MoreLinq;
using NodaTime;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services.Detection;
using SRHWiscMano.Core.Services.Report;

namespace SRHWiscMano.Core.Services.Diagnostics
{
    public class DetectionDiagnostics : IDetectionDiagnostics, ISensorChartParams
    {
        private readonly IExamination exam;
        private readonly ITimeFrame timeFrame;
        private Dictionary<RegionType, RegionDiagnostics> regions;
        private Interval timeRange;

        public DetectionDiagnostics(IExamination exam, ITimeFrame timeFrame, Duration window)
        {
            this.exam = exam;
            this.timeFrame = timeFrame;
            timeRange = timeFrame.Time.AtCenterOfDuration(window, timeFrame.ExamData.TotalTime());
            regions = new Dictionary<RegionType, RegionDiagnostics>();
        }

        private string Title => string.Join("-", exam.Title, timeFrame.Text);

        /// <summary>
        /// 기존의 time range에서 입력받은 range를 더한다
        /// </summary>
        /// <param name="timeRange"></param>
        public void ExpandTimeRange(Interval timeRange)
        {
            this.timeRange = this.timeRange.Union(timeRange);
        }

        public void AddMarker(
            RegionType region,
            Instant time,
            int sensorIndex,
            DetectionMarkerType markerType)
        {
            SensorChart sensorChart = GetSensorChart(region, sensorIndex);
            if (sensorChart.Markings.Any(m => m.Item1 == time))
                return;
            sensorChart.Markings.Add(Tuple.Create(time, markerType));
        }

        public void SetThreshold(RegionType region, int sensorIndex, double threshold)
        {
            GetSensorChart(region, sensorIndex).Threshold = threshold;
        }

        public ITimeFrame TimeFrame => timeFrame;

        public Interval TimeRange => timeRange;

        public void Generate()
        {
            Duration duration = Duration.FromMilliseconds(250L);
            timeRange = new Interval(timeRange.Start - duration, timeRange.End + duration);
            string path2 = Title + ".html";
            string str = Path.Combine(Path.GetTempPath(), path2);
            using (StreamWriter writer = new StreamWriter(str, false, Encoding.UTF8))
            {
                WriteHtml(writer);
                writer.Flush();
            }

            Process.Start(str);
        }

        private SensorChart GetSensorChart(RegionType region, int sensorIndex)
        {
            RegionDiagnostics regionDiagnostics;
            if (!regions.TryGetValue(region, out regionDiagnostics))
            {
                regionDiagnostics = new RegionDiagnostics(region);
                regions.Add(region, regionDiagnostics);
            }

            return regionDiagnostics.GetSensorChart(this, sensorIndex);
        }

        private TextWriter WriteHtml(TextWriter writer)
        {
            writer.WriteLine("<!DOCTYPE html>");
            writer.WriteLine("<html>");
            writer.WriteLine("<head>");
            WriteHead(writer);
            writer.WriteLine("</head>");
            writer.WriteLine("<body>");
            writer.WriteLine("\t<div class=\"container\">");
            writer.WriteLine("\t\t<div class=\"row\">");
            writer.WriteLine("\t\t\t<div class=\"col-xs-12 col-sm-12\">");
            writer.WriteLine("\t\t\t\t<h1>{0}</h1>", Title);
            writer.WriteLine("\t\t\t</div>");
            writer.WriteLine("\t\t</div>");
            foreach (RegionDiagnostics regionDiagnostics in regions.Values)
            {
                writer.WriteLine("\t\t<div class=\"row\">");
                writer.WriteLine("\t\t\t<hr />");
                writer.WriteLine("\t\t\t<div class=\"col-xs-12 col-sm-12\">");
                writer.WriteLine("\t\t\t\t<h2>{0}</h2>", regionDiagnostics.Region.ToString());
                writer.WriteLine("\t\t\t</div>");
                writer.WriteLine("\t\t</div>");
                regionDiagnostics.SensorCharts.Values.ForEach(c => c.WriteHtml(writer));
            }

            writer.WriteLine("\t</div>");
            WriteFooter(writer, regions.Values.SelectMany(r => r.SensorCharts.Values));
            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
            writer.WriteLine();
            return writer;
        }

        private TextWriter WriteHead(TextWriter writer)
        {
            writer.WriteLine("<title>{0} Diagnostics</title>", Title);
            writer.WriteLine("<style>");
            // writer.WriteLine(Resources.bootstrap_min_css);
            // writer.WriteLine(Resources.bootstrap_theme_min_css);
            writer.WriteLine("</style>");
            writer.WriteLine("<style>");
            writer.WriteLine(".chart { height: 150px }");
            writer.WriteLine("</style>");
            return writer;
        }

        private TextWriter WriteFooter(TextWriter writer, IEnumerable<Chart> charts)
        {
            writer.WriteLine("<script type=\"text/javascript\">");
            // writer.WriteLine(Resources.jquery_min_js);
            // writer.WriteLine(Resources.jquery_flot_min_js);
            writer.WriteLine();
            writer.WriteLine("</script>");
            writer.WriteLine("<script type=\"text/javascript\">");
            charts.ForEach(c => writer.WriteLine(c.RenderJS()));
            writer.WriteLine("</script>");
            return writer;
        }

        private class RegionDiagnostics
        {
            public RegionDiagnostics(RegionType region)
            {
                Region = region;
                SensorCharts = new Dictionary<int, SensorChart>();
            }

            public RegionType Region { get; }

            public Dictionary<int, SensorChart> SensorCharts { get; }

            public SensorChart GetSensorChart(
                ISensorChartParams sensorChartParams,
                int sensorIndex)
            {
                SensorChart sensorChart;
                if (!SensorCharts.TryGetValue(sensorIndex, out sensorChart))
                {
                    sensorChart = new SensorChart(sensorChartParams, Region, sensorIndex);
                    SensorCharts.Add(sensorIndex, sensorChart);
                }

                return sensorChart;
            }
        }
    }
}