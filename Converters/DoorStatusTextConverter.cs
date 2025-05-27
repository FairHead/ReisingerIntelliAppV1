using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.Converters
{
    public class DoorStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOpen = value is bool b && b;
            return isOpen
                ? "🚪OPEN – No configuration possible"
                : "🚪CLOSED – Configuration possible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
