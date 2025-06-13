using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System.Diagnostics;

namespace ReisingerIntelliAppV1.Converters
{
    public class RelativePositionToLayoutBoundsConverter : IMultiValueConverter
    {
        // Match these values with the WidthRequest and HeightRequest in PlacedDeviceControl.xaml
        private const int CONTROL_WIDTH = 260;
        private const int CONTROL_HEIGHT = 60;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is double x &&
                values[1] is double y)
            {
                // Use fixed size that matches the PlacedDeviceControl dimensions
                Debug.WriteLine($"[RelativePositionToLayoutBoundsConverter] Converting position: ({x}, {y})");
                return new Rect(x, y, CONTROL_WIDTH, CONTROL_HEIGHT);
            }

            Debug.WriteLine("[RelativePositionToLayoutBoundsConverter] Default position used");
            return new Rect(0.5, 0.5, CONTROL_WIDTH, CONTROL_HEIGHT);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}