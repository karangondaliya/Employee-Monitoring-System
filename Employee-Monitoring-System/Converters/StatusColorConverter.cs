using System.Globalization;

namespace Employee_Monitoring_System.Converters
{
    public class TaskStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "completed" => "#4CAF50",   // Green
                    "in progress" => "#2196F3", // Blue
                    "pending" => "#FF9800",     // Orange
                    "overdue" => "#F44336",     // Red
                    _ => "#9E9E9E"              // Grey (default)
                };
            }
            return "#9E9E9E";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProjectStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "completed" => "#4CAF50",   // Green
                    "in progress" => "#2196F3", // Blue
                    "planning" => "#FF9800",    // Orange
                    "on hold" => "#F44336",     // Red
                    _ => "#9E9E9E"              // Grey (default)
                };
            }
            return "#9E9E9E";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}