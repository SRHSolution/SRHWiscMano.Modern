using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SRHWiscMano.App.Data
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
