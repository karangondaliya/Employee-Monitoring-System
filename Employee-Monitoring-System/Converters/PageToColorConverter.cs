using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Converters
{
    public class PageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string pageName = parameter as string;
            string activePage = SidebarViewModel.Instance.ActivePage;

            return (pageName != null && pageName.ToLower() == activePage.ToLower()) ? Color.FromArgb("#1ABC9C") : Color.FromArgb("#34495E");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
