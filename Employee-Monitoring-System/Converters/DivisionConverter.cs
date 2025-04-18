using System.Globalization;

namespace Employee_Monitoring_System.Views.Components
{
    public class DivisionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index && parameter is string divisorString && int.TryParse(divisorString, out int divisor))
            {
                return index / divisor;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}