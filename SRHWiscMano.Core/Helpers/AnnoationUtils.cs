using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Annotations;
using Shouldly;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Helpers
{
    public static class AnnoationUtils
    {
        public static void CreateVLineAnnotation(ITimeFrame timeFrame, bool draggable, PlotModel? model)
        {
            var msec = timeFrame.Time.ToMillisecondsFromEpoch() / 10;
            var la = new LineAnnotation
            {
                Type = LineAnnotationType.Vertical, X = msec, LineStyle = LineStyle.Solid, ClipByYAxis = true,
                Text = timeFrame.Text, TextOrientation = AnnotationTextOrientation.Horizontal,
                Tag = timeFrame.Id
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

            model!.Annotations.Add(la);
        }
    }
}
