using System.Windows.Media;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;

namespace SRHWiscMano.Core.Helpers
{
    public static class PlotModelExtensions
    {
        public static PlotModel? CloneModel(this PlotModel model)
        {
            var serialized = JsonConvert.SerializeObject(model);
            return JsonConvert.DeserializeObject<PlotModel>(serialized);
        }

        public static void ApplyTheme(this PlotModel model, Color backColor, Color foreColor)
        {
            model.Background = OxyColor.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B);
            model.TextColor = OxyColor.FromArgb(foreColor.A, foreColor.R, foreColor.G, foreColor.B);
            model.PlotAreaBorderColor = OxyColors.Gray;
            var linAxes = model.Axes.OfType<LinearAxis>();
            foreach (var axis in linAxes)
            {
                axis.TicklineColor = model.TextColor;
                axis.MinorTicklineColor = model.TextColor;
            }
            model.InvalidatePlot(false);
        }

    }
}
