using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.Services;

namespace Employee_Monitoring_System.ViewModels
{
    public class EmployeesViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        private bool _isLoading;
        private string _searchQuery;
        private string _selectedDepartment;
        private ICommand _refreshCommand;
        private ICommand _newEmployeeCommand;
        private ICommand _viewDetailsCommand;

        public ObservableCollection<Employee> Employees { get; } = new();
        public ObservableCollection<string> Departments { get; } = new()
        {
            "All Departments",
            "IT",
            "HR",
            "Finance",
            "Marketing",
            "Operations"
        };

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    LoadEmployeesAsync().ConfigureAwait(false);
                }
            }
        }

        public string SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    LoadEmployeesAsync().ConfigureAwait(false);
                }
            }
        }

        public bool IsAdmin { get; set; }

        public ICommand RefreshCommand
        {
            get => _refreshCommand;
            set => SetProperty(ref _refreshCommand, value);
        }

        public ICommand NewEmployeeCommand
        {
            get => _newEmployeeCommand;
            set => SetProperty(ref _newEmployeeCommand, value);
        }

        public ICommand ViewDetailsCommand
        {
            get => _viewDetailsCommand;
            set => SetProperty(ref _viewDetailsCommand, value);
        }

        // Public parameterless constructor for XAML
        public EmployeesViewModel()
        {
            _employeeService = new EmployeeService();
            Initialize();
        }

        // Constructor with dependency injection
        public EmployeesViewModel(EmployeeService employeeService)
        {
            _employeeService = employeeService;
            Initialize();
        }

        private void Initialize()
        {
            // Set user role (replace with actual role-fetching logic)
            var userRole = Preferences.Get("UserRole", "Employee");
            IsAdmin = userRole == "Admin";

            // Initialize commands
            RefreshCommand = new Command(async () => await LoadEmployeesAsync());
            NewEmployeeCommand = new Command(async () => await OnNewEmployee());
            ViewDetailsCommand = new Command<Employee>(async (employee) => await OnViewDetails(employee));

            // Initial load
            LoadEmployeesAsync().ConfigureAwait(false);
        }

        private async Task LoadEmployeesAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                // Check for authentication token
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    // Redirect to login if no token is found
                    await Shell.Current.GoToAsync("//LoginPage");
                    return;
                }

                var department = SelectedDepartment == "All Departments" ? null : SelectedDepartment;
                var employees = await _employeeService.GetEmployeesAsync(SearchQuery, department);

                // Debug output to confirm data is received
                System.Diagnostics.Debug.WriteLine($"Received {employees?.Count ?? 0} employees from service");
                foreach (var employee in employees)
                {
                    System.Diagnostics.Debug.WriteLine($"Employee: {employee.Id} - {employee.FullName} - {employee.Position}");
                }

                // Update UI on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Employees.Clear();
                    if (employees != null)
                    {
                        foreach (var employee in employees)
                        {
                            Employees.Add(employee);
                        }
                    }
                    // Force UI refresh
                    OnPropertyChanged(nameof(Employees));
                    System.Diagnostics.Debug.WriteLine($"Employees collection updated with {Employees.Count} items");
                });
            }
            catch (UnauthorizedAccessException)
            {
                // Handle unauthorized access (invalid/expired token)
                await Application.Current.MainPage.DisplayAlert("Session Expired",
                    "Please log in again.", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employees: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Unable to load employees. Please try again later.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnNewEmployee()
        {
            try
            {
                // Navigate to new employee page or show modal
                await Shell.Current.GoToAsync("//EmployeeDetailsPage?mode=new");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to new employee page: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Unable to create new employee.", "OK");
            }
        }

        private async Task OnViewDetails(Employee employee)
        {
            if (employee != null)
            {
                try
                {
                    // Navigate to employee details page
                    await Shell.Current.GoToAsync($"//EmployeeDetailsPage?id={employee.Id}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error navigating to employee details: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Unable to load employee details.", "OK");
                }
            }
        }
    }
}