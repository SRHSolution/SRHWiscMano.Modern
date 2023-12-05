using OxyPlot;
using OxyPlot.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Annotations;

namespace SRHWiscMano.Core.Control
{
    public class LineAnnotationManipulator : MouseManipulator
    {
        private readonly IPlotView view;
        private LineAnnotation? annotation;

        public LineAnnotationManipulator(IPlotView view) : base(view)
        {
            this.view = view;
        }

        public bool IsVertical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether panning is enabled.
        /// </summary>
        private bool IsPanEnabled { get; set; }

        /// <summary>
        /// Occurs when a manipulation is complete.
        /// </summary>
        /// <param name="e">The <see cref="OxyInputEventArgs" /> instance containing the event data.</param>
        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);
            if (!this.IsPanEnabled)
            {
                return;
            }


            annotation!.StrokeThickness /= 5;
            view.InvalidatePlot(false);
            this.View.SetCursorType(CursorType.Default);

            annotation = null;
            e.Handled = true;
        }

        /// <summary>
        /// Occurs when the input device changes position during a manipulation.
        /// </summary>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);
            if (!this.IsPanEnabled)
            {
                return;
            }

            if (IsVertical)
            {
                annotation!.X = annotation.InverseTransform(e.Position).X;
                view.InvalidatePlot(false);
            }
            else
            {
                annotation!.Y = annotation.InverseTransform(e.Position).Y;
                view.InvalidatePlot(false);
            }

            this.PlotView.InvalidatePlot(false);
            e.Handled = true;
        }

        /// <summary>
        /// Occurs when an input device begins a manipulation on the plot.
        /// </summary>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public override void Started(OxyMouseEventArgs e)
        {
            base.Started(e);

            var args = new HitTestArguments(e.Position, 10);
            var firstHit = view.ActualModel.HitTest(args).FirstOrDefault(x => x.Element is LineAnnotation);

            if (firstHit != null)
            {
                annotation = firstHit.Element as LineAnnotation;
                annotation!.StrokeThickness *= 5;

                this.View.SetCursorType(CursorType.Pan);
                
                view.InvalidatePlot(false);
                IsPanEnabled = true;
                e.Handled = true;
            }
            else
            {
                IsPanEnabled = false;
                annotation = null;
            }
        }
    }
}
