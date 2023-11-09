using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace SRHWiscMano.App.Data
{
    public class TabIndexChangeMessage : ValueChangedMessage<int>
    {
        public TabIndexChangeMessage(int index) : base(index)
        {
        }
    }
}
