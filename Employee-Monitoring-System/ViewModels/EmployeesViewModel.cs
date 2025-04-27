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
        private ICommand _refreshCommand;
        private ICommand _newEmployeeCommand;
        private ICommand _viewDetailsCommand;
        private bool _hasConnectionError = false;

        public ObservableCollection<Employee> Employees { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasConnectionError
        {
            get => _hasConnectionError;
            set => SetProperty(ref _hasConnectionError, value);
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

        public ICommand RetryConnectionCommand { get; private set; }

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
            RetryConnectionCommand = new Command(async () => await LoadEmployeesAsync());

            // Initial load
            LoadEmployeesAsync().ConfigureAwait(false);
        }

        private async Task LoadEmployeesAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                HasConnectionError = false;

                // Check for authentication token
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    // Redirect to login if no token is found
                    await Shell.Current.GoToAsync("//LoginPage");
                    return;
                }

                var employees = await _employeeService.GetEmployeesAsync(SearchQuery);

                // Debug output to confirm data is received
                System.Diagnostics.Debug.WriteLine($"Received {employees?.Count ?? 0} employees from service");

                // Process employees to ensure valid data
                if (employees != null)
                {
                    foreach (var employee in employees)
                    {
                        // Ensure phone is not null
                        if (string.IsNullOrEmpty(employee.Phone))
                        {
                            employee.PhoneNumber = "Not provided";
                        }

                        // Ensure status is not null
                        if (string.IsNullOrEmpty(employee.Status))
                        {
                            employee.IsActive = true;
                        }
                    }
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
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection error loading employees: {ex.Message}");
                HasConnectionError = true;
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