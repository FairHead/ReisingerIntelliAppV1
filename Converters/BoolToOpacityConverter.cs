using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ReisingerIntelliAppV1.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAlreadySaved && isAlreadySaved)
                return 0.8; // Ausgegraut
            return 1.0; // Normal
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}