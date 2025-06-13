using System;
using System.Collections;
using System.Globalization;

namespace ReisingerIntelliAppV1.Converters
{
    public class AnyTrueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if any value is true
            foreach (var value in values)
            {
                // Skip null values
                if (value == null)
                    continue;

                // Try to convert the value to bool
                if (value is bool boolValue && boolValue)
                {
                    return true;
                }
            }

            // If no true values were found
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}