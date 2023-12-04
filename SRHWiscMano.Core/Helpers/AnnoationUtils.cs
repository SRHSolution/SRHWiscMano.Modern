using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Annotations;
using Shouldly;

namespace SRHWiscMano.Core.Helpers
{
    public static class AnnoationUtils
    {
        public static void CreateVLineAnnotation(double xPos, string text, bool draggable, PlotModel? model)
        {
            var la = new LineAnnotation
            {
                Type = LineAnnotationType.Vertical, X = xPos, LineStyle = LineStyle.Solid, ClipByYAxis = true,
                Text = text, TextOrientation = AnnotationTextOrientation.Horizontal
            };

            if (draggable)
            {
                model.ShouldNotBeNull("Draggable Line annotation requires plot model");

                la.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton != OxyMouseButton.Left)
                    {
                        return;
                    }

                    la.StrokeThickness *= 5;
                    model.InvalidatePlot(false);
                    e.Handled = true;
                };

                // Handle mouse movements (note: this is only called when the mousedown event was handled)
                la.MouseMove += (s, e) =>
                {
                    la.X = la.InverseTransform(e.Position).X;
                    model.InvalidatePlot(false);
                    e.Handled = true;
                };

                la.MouseUp += (s, e) =>
                {
                    la.StrokeThickness /= 5;
                    model.InvalidatePlot(false);
                    e.Handled = true;
                };
            }

            model.Annotations.Add(la);
        }
    }
}
