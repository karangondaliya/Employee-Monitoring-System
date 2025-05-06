using System.Windows.Input;
using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.Services;
using MvvmHelpers;
using System.Linq;

namespace Employee_Monitoring_System.ViewModels
{
    public class EmployeeDetailsViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        private Employee _employee;
        private bool _isLoading;
        private string _statusText;
        private string _statusColor;
        private string _activationButtonText;
        private string _activationButtonColor;
        private bool _isAdmin;
        private bool _hasImage;
        private string _profileImage;

        public Employee Employee
        {
            get => _employee;
            set => SetProperty(ref _employee, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public string ActivationButtonText
        {
            get => _activationButtonText;
            set => SetProperty(ref _activationButtonText, value);
        }

        public string ActivationButtonColor
        {
            get => _activationButtonColor;
            set => SetProperty(ref _activationButtonColor, value);
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        public bool HasImage
        {
            get => _hasImage;
            set => SetProperty(ref _hasImage, value);
        }

        public string ProfileImage
        {
            get => _profileImage;
            set => SetProperty(ref _profileImage, value);
        }

        public bool HasTasks => Employee?.Tasks?.Count > 0;

        public bool HasProjects => Employee?.Projects?.Count > 0;

        public bool HasSkills => Employee?.TechnicalSkills?.Count > 0;

        public bool HasCertifications => Employee?.Certifications?.Count > 0;

        public ICommand GoBackCommand { get; set; }
        public ICommand EditEmployeeCommand { get; set; }
        public ICommand ToggleActivationCommand { get; set; }
        public ICommand ViewTaskDetailsCommand { get; set; }
        public ICommand ViewProjectDetailsCommand { get; set; }

        // Public parameterless constructor for XAML
        public EmployeeDetailsViewModel()
        {
            _employeeService = new EmployeeService();
            Initialize();
        }

        // Constructor with dependency injection
        public EmployeeDetailsViewModel(EmployeeService employeeService)
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
            GoBackCommand = new Command(async () => await OnGoBack());
            EditEmployeeCommand = new Command(async () => await OnEditEmployee());
            ToggleActivationCommand = new Command(async () => await OnToggleActivation());
            ViewTaskDetailsCommand = new Command<UserTask>(async (task) => await OnViewTaskDetails(task));
            ViewProjectDetailsCommand = new Command<UserProject>(async (project) => await OnViewProjectDetails(project));
        }

        public async void LoadEmployee(int employeeId)
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

                // Get employee details from service
                var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

                if (employee != null)
                {
                    Employee = employee;

                    // Set status display based on IsActive
                    UpdateStatusDisplay();

                    // Check if employee has profile image
                    HasImage = !string.IsNullOrEmpty(Employee.ProfileImageBase64);
                    if (HasImage)
                    {
                        ProfileImage = Employee.ProfileImageBase64;
                    }

                    // Set activation button text and color based on current status
                    UpdateActivationButton();

                    // Notify UI to update derived properties
                    OnPropertyChanged(nameof(HasTasks));
                    OnPropertyChanged(nameof(HasProjects));
                    OnPropertyChanged(nameof(HasSkills));
                    OnPropertyChanged(nameof(HasCertifications));
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Employee not found. Please try again.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employee: {ex.Message}");

                await Application.Current.MainPage.DisplayAlert("Error",
                    "Unable to load employee details. Please try again later.", "OK");

                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateStatusDisplay()
        {
            if (Employee != null)
            {
                StatusText = Employee.Status;
                StatusColor = Employee.IsActive ? "#4CAF50" : "#F44336"; // Green for active, red for inactive
            }
        }

        private void UpdateActivationButton()
        {
            if (Employee != null)
            {
                // Set button text and color based on current status
                ActivationButtonText = Employee.IsActive ? "Deactivate" : "Activate";
                ActivationButtonColor = Employee.IsActive ? "#F44336" : "#4CAF50"; // Red for deactivate, green for activate
            }
        }

        private async Task OnGoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async Task OnEditEmployee()
        {
            if (Employee != null)
            {
                // Navigate to edit employee page with the current employee ID
                await Shell.Current.GoToAsync($"EditEmployeePage?id={Employee.Id}");
            }
        }

        private async Task OnToggleActivation()
        {
            if (Employee != null)
            {
                try
                {
                    string action = Employee.IsActive ? "deactivate" : "activate";
                    bool confirmed = await Application.Current.MainPage.DisplayAlert(
                        $"{(Employee.IsActive ? "Deactivate" : "Activate")} Employee",
                        $"Are you sure you want to {action} {Employee.FullName}?",
                        "Yes", "No");

                    if (confirmed)
                    {
                        // Toggle activation status
                        Employee.IsActive = !Employee.IsActive;

                        // Update employee in database
                        await _employeeService.UpdateEmployeeAsync(Employee.Id, Employee);

                        // Update UI
                        UpdateStatusDisplay();
                        UpdateActivationButton();

                        await Application.Current.MainPage.DisplayAlert(
                            "Success",
                            $"Employee {action}d successfully.",
                            "OK");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error toggling activation: {ex.Message}");

                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Failed to {(Employee.IsActive ? "deactivate" : "activate")} employee.",
                        "OK");

                    // Revert the change since it failed
                    Employee.IsActive = !Employee.IsActive;
                    UpdateStatusDisplay();
                    UpdateActivationButton();
                }
            }
        }

        private async Task OnViewTaskDetails(UserTask task)
        {
            if (task != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Task Details",
                    $"Task: {task.TaskName}\nStatus: {task.Status}\nID: {task.TaskId}",
                    "OK");

                // In a real implementation, you might navigate to a task details page:
                // await Shell.Current.GoToAsync($"TaskDetailsPage?id={task.TaskId}");
            }
        }

        private async Task OnViewProjectDetails(UserProject project)
        {
            if (project != null)
            {
                // Navigate to project details page
                await Shell.Current.GoToAsync($"ProjectDetailsPage?id={project.ProjectId}");
            }
        }
    }
}