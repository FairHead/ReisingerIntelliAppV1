using System;
using System.Globalization;

namespace ReisingerIntelliAppV1.Converters
{
    public class PositioningModeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isPositioningMode = (bool)value;
            return isPositioningMode ? Colors.Orange : Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}