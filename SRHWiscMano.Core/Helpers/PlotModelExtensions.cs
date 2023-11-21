using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OxyPlot;

namespace SRHWiscMano.Core.Helpers
{
    public static class PlotModelExtensions
    {
        public static PlotModel? CloneModel(this PlotModel model)
        {
            var serialized = JsonConvert.SerializeObject(model);
            return JsonConvert.DeserializeObject<PlotModel>(serialized);
        }

    }
}
