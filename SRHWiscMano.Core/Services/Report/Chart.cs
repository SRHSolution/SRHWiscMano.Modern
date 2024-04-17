using System.IO;

namespace SRHWiscMano.Core.Services.Report
{
    internal abstract class Chart
    {
        public Chart(
            string id,
            string title,
            string xaxisLabel,
            string yaxisLabel,
            int yaxisLabelOffset)
        {
            Id = id;
            Title = title;
            XAxisLabel = xaxisLabel;
            YAxisLabel = yaxisLabel;
            YAxisLabelOffset = yaxisLabelOffset;
        }

        public string Id { get; set; }

        public string Title { get; set; }

        public string XAxisLabel { get; set; }

        public string YAxisLabel { get; set; }

        public int YAxisLabelOffset { get; set; }

        public virtual void WriteHtml(TextWriter writer)
        {
            writer.WriteLine("\t\t\t<div class=\"chart-box\">");
            writer.WriteLine("\t\t\t\t<h4 class=\"text-center\">{0}</h4>", Title);
            writer.WriteLine(
                "\t\t\t\t<div class=\"vertical-text y-axis-label\" style=\"padding-top:{1}px;\"><div class=\"vertical-text__inner\">{0}</div></div>",
                YAxisLabel, YAxisLabelOffset);
            writer.WriteLine("\t\t\t\t<div class=\"chart-right\">");
            writer.WriteLine("\t\t\t\t\t<div id=\"{0}\" class=\"chart\"></div>", Id);
            writer.WriteLine("\t\t\t\t\t<div class=\"x-axis-label\">{0}</div>", XAxisLabel);
            writer.WriteLine("\t\t\t\t</div>");
            writer.WriteLine("\t\t\t</div>");
        }

        public abstract string RenderJS();
    }
}