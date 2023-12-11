using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SRHWiscMano.Core.Data
{

    public class AppBaseThemeChangedMessage : ValueChangedMessage<Tuple<Color, Color>>
    {
        public AppBaseThemeChangedMessage(Tuple<Color, Color> colors) : base(colors)
        {
        }
    }

    public class AppSchemeColorChangedMessage : ValueChangedMessage<Color>
    {
        public AppSchemeColorChangedMessage(Color color) : base(color)
        {
        }
    }
}
