using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Employee_Monitoring_System.Converters
{
    public class HalfWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                // Get half the width minus spacing for 2 columns
                // Account for horizontal spacing (16) and container padding (48 = 24 left + 24 right)
                double availableWidth = width - 48;
                double itemWidth = (availableWidth - 16) / 2;
                return itemWidth;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}