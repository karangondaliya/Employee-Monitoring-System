using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace Employee_Monitoring_System.Views
{
    public partial class EmployeesPage : ContentPage
    {
        public EmployeesPage()
        {
            InitializeComponent();

            // Add debugging for binding context and data
            this.Appearing += (s, e) =>
            {
                Debug.WriteLine("EmployeesPage is appearing");

                if (BindingContext is Employee_Monitoring_System.ViewModels.EmployeesViewModel vm)
                {
                    Debug.WriteLine($"BindingContext is EmployeesViewModel with {vm.Employees.Count} employees");

                    foreach (var employee in vm.Employees)
                    {
                        Debug.WriteLine($"Employee in view: {employee.Id} - {employee.FullName}");
                    }
                }
                else
                {
                    Debug.WriteLine("BindingContext is NOT EmployeesViewModel");
                }

                if (EmployeesCollectionView != null)
                {
                    Debug.WriteLine($"CollectionView ItemsSource count: {EmployeesCollectionView.ItemsSource?.Cast<object>().Count() ?? 0}");
                    Debug.WriteLine($"CollectionView IsVisible: {EmployeesCollectionView.IsVisible}");
                }
            };
        }
    }
}