using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Employee_Monitoring_System.Converters
{
    public class PageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return "#2C3E50"; // Default button color

            string activePage = value.ToString();
            string buttonPage = parameter.ToString();

            return activePage.Equals(buttonPage, StringComparison.OrdinalIgnoreCase) ? "#1ABC9C" : "#2C3E50";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
