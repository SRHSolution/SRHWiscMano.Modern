using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Converters
{
    public class FavoritePaletteConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new FavoritePalette()
            {
                PaletteName = (string)values[0],
                UpperValue = int.Parse(values[1].ToString()),
                LowerValue = int.Parse(values[2].ToString())
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { };
        }
    }
}
