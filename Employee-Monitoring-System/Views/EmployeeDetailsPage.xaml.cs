using Microsoft.Maui.Controls;
using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Views
{
    [QueryProperty(nameof(EmployeeId), "id")]
    public partial class EmployeeDetailsPage : ContentPage
    {
        private int _employeeId;

        public int EmployeeId
        {
            get => _employeeId;
            set
            {
                _employeeId = value;
                LoadEmployee(_employeeId);
            }
        }

        public EmployeeDetailsPage()
        {
            InitializeComponent();

            // Add debugging for binding context and navigation
            this.Appearing += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"EmployeeDetailsPage appearing with ID: {_employeeId}");
            };
        }

        private void LoadEmployee(int employeeId)
        {
            try
            {
                if (BindingContext is EmployeeDetailsViewModel viewModel)
                {
                    System.Diagnostics.Debug.WriteLine($"Loading employee ID: {employeeId}");
                    viewModel.LoadEmployee(employeeId);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("BindingContext is not EmployeeDetailsViewModel");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadEmployee: {ex.Message}");
            }
        }
    }
}