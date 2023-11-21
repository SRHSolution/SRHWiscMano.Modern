using System.Globalization;
using System.Windows.Data;

namespace SRHWiscMano.Core.Converters
{
    public class StringToDoubleConverter : IValueConverter
    {

        /// <summary>
        /// Source to Target
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && double.TryParse(stringValue, out double result))
            {
                return result;
            }
            return 0.0; // Or your default value
        }


        /// <summary>
        /// Target to Source
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue.ToString(culture);
            }

            return null;
        }
    }
}
