using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using Employee_Monitoring_System.ViewModels;
using Employee_Monitoring_System.Models;

namespace Employee_Monitoring_System.Converters
{
    public class PageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as SidebarItem;
            var vm = SidebarViewModel.Instance;

            if (item == null || vm == null || string.IsNullOrEmpty(vm.ActivePage))
                return Colors.Transparent;

            return item.Title.ToLower() == vm.ActivePage.ToLower()
                ? Color.FromArgb("#1ABC9C")
                : Colors.Transparent;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
