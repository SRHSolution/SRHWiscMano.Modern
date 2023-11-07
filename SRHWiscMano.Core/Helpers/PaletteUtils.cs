using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Helpers
{
    public static class PaletteUtils
    {
        /// <summary>
        /// OxyPalettes의 함수를 기반으로 Palettes 를 생성하여 전달한다.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, OxyPalette> GetPredefinedPalettes()
        {
            // List to store the palettes
            var palettes = new Dictionary<string, OxyPalette>();

            // Get all static methods of the OxyPalettes class
            var methods = typeof(OxyPalettes).GetMethods(BindingFlags.Public | BindingFlags.Static);

            // Loop through each method
            foreach (var method in methods)
            {
                // Check if the method returns an OxyPalette
                if (method.ReturnType == typeof(OxyPalette))
                {
                    // Assume the method takes either no parameters or a single integer parameter
                    var parameters = method.GetParameters();

                    // If there are no parameters, invoke the method directly
                    if (parameters.Length == 0)
                    {
                        var palette = (OxyPalette)method.Invoke(null, null);
                        palettes.Add(method.Name.Replace("get_", ""), palette);
                    }
                    // If there is one integer parameter, invoke it with a default value (e.g., 10)
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
                    {
                        if ( parameters[0].HasDefaultValue)
                        {
                            var palette = (OxyPalette)method.Invoke(null, new object[] { parameters[0].DefaultValue });
                            palettes.Add(method.Name, palette);
                        }
                        else
                        {
                            var palette = (OxyPalette)method.Invoke(null, new object[] { 1024 });
                            palettes.Add(method.Name, palette);
                        }
                    }
                }
            }

            return palettes;
        }

        
    }
}
