using System.Globalization;

namespace Employee_Monitoring_System.Converters
{
    public class PageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This converter checks if the current menu item is selected/active

            if (value == null || parameter == null)
                return Colors.Transparent;

            var menuItem = value as Models.SidebarItem;
            var activePage = parameter as string;

            if (menuItem == null || string.IsNullOrEmpty(activePage))
                return Colors.Transparent;

            // If the title matches the active page, return a highlight color
            if (menuItem.Title == activePage)
                return Color.FromArgb("#2C5282"); // Deeper blue for selected item

            return Colors.Transparent; // Default background
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}