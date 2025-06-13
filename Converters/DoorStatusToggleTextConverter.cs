using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ReisingerIntelliAppV1.Converters
{
    public class DoorStatusToggleTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOpen = value is bool b && b;
            return isOpen ? "Close" : "Open";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}