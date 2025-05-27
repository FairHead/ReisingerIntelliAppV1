using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ReisingerIntelliAppV1.Converters
{
    public class DoorStatusMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOpen = value is bool b && b;
            return isOpen
                ? "🚪 Door is OPEN – No Configuration possible"
                : "✅ Door is CLOSED – Configuration possible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}