using CommunityToolkit.Mvvm.Messaging.Messages;
using OxyPlot;

namespace SRHWiscMano.Core.Data
{
    public class SensorBoundsChangedMessageArg
    {
        public double MinBound;
        public double MaxBound;
    }

    public class SensorBoundsChangedMessage : ValueChangedMessage<SensorBoundsChangedMessageArg>
    {
        public SensorBoundsChangedMessage(SensorBoundsChangedMessageArg arg) : base(arg)
        {
        }
    }
}
