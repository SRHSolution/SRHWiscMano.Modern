using CommunityToolkit.Mvvm.Messaging.Messages;
using OxyPlot;

namespace SRHWiscMano.Core.Data
{
    public class PaletteChangedMessageArg
    {
        public OxyPalette palette;
        public double Minimum;
        public double Maximum;
        public OxyColor HighColor { get; set; }
        public OxyColor LowColor { get; set; }
    }

    public class PaletteChangedMessageMessage : ValueChangedMessage<PaletteChangedMessageArg>
    {
        public PaletteChangedMessageMessage(PaletteChangedMessageArg arg) : base(arg)
        {
        }
    }
}
