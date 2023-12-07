using OxyPlot.Wpf;

namespace SRHWiscMano.App.Controls
{
    public class SRHPlotView : PlotView
    {
        public bool SuspendRender { get; set; }

        public object SyncPlot { get; private set; } = new object();

        protected override void RenderOverride()
        {
            if (SuspendRender)
                return;

            base.RenderOverride();
        }
    }
}
