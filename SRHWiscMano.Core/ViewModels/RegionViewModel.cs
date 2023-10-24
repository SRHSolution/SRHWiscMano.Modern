using SRHWiscMano.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.ViewModels
{
    public partial class RegionViewModel : ViewModelBase, IRegionViewModel
    {
        private Color color;
        private IRegion region;
    }
}
