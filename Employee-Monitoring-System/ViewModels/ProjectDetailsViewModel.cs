using System.Windows.Input;
using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.Services;
using MvvmHelpers;

namespace Employee_Monitoring_System.ViewModels
{
    public class ProjectDetailsViewModel : BaseViewModel
    {
        private readonly ProjectService _projectService;
        private Project _project;
        private bool _isLoading;
        private string _statusColor;
        private string _deadlineColor;
        private string _daysRemaining;
        private bool _isAdmin;

        public Project Project
        {
            get => _project;
            set => SetProperty(ref _project, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public string DeadlineColor
        {
            get => _deadlineColor;
            set => SetProperty(ref _deadlineColor, value);
        }

        public string DaysRemaining
        {
            get => _daysRemaining;
            set => SetProperty(ref _daysRemaining, value);
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        public ICommand GoBackCommand { get; set; }
        public ICommand EditProjectCommand { get; set; }
        public ICommand UpdateProgressCommand { get; set; }

        // Public parameterless constructor for XAML
        public ProjectDetailsViewModel()
        {
            _projectService = new ProjectService();
            Initialize();
        }

        // Constructor with dependency injection
        public ProjectDetailsViewModel(ProjectService projectService)
        {
            _projectService = projectService;
            Initialize();
        }

        private void Initialize()
        {
            // Set user role (replace with actual role-fetching logic)
            var userRole = Preferences.Get("UserRole", "Employee");
            IsAdmin = userRole == "Admin";

            // Initialize commands
            GoBackCommand = new Command(async () => await OnGoBack());
            EditProjectCommand = new Command(async () => await OnEditProject());
            UpdateProgressCommand = new Command(async () => await OnUpdateProgress());
        }

        public async void LoadProject(int projectId)
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

                // Get project details from service
                var project = await _projectService.GetProjectByIdAsync(projectId);

                if (project != null)
                {
                    Project = project;

                    // Set status color based on project status
                    StatusColor = GetStatusColor(Project.Status);

                    // Calculate days remaining and set color
                    SetDeadlineInfo();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Project not found. Please try again.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading project: {ex.Message}");

                await Application.Current.MainPage.DisplayAlert("Error",
                    "Unable to load project details. Please try again later.", "OK");

                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "planning" => "#FF9800",   // Orange
                "in progress" => "#2196F3", // Blue
                "completed" => "#4CAF50",   // Green
                _ => "#9E9E9E"              // Grey (default)
            };
        }

        private void SetDeadlineInfo()
        {
            if (Project != null)
            {
                var today = DateTime.Today;
                var deadline = Project.Deadline.Date;
                var daysLeft = (deadline - today).Days;

                if (daysLeft < 0)
                {
                    // Overdue
                    DaysRemaining = "Overdue by " + Math.Abs(daysLeft) + (Math.Abs(daysLeft) == 1 ? " day" : " days");
                    DeadlineColor = "#F44336"; // Red
                }
                else if (daysLeft == 0)
                {
                    // Due today
                    DaysRemaining = "Due today";
                    DeadlineColor = "#FF9800"; // Orange
                }
                else
                {
                    // Future deadline
                    DaysRemaining = daysLeft + (daysLeft == 1 ? " day" : " days") + " remaining";

                    if (daysLeft <= 7)
                        DeadlineColor = "#FF9800"; // Orange (due soon)
                    else
                        DeadlineColor = "#4CAF50"; // Green (plenty of time)
                }
            }
        }

        private async Task OnGoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async Task OnEditProject()
        {
            if (Project != null)
            {
                // Navigate to edit project page with the current project ID
                await Shell.Current.GoToAsync($"EditProjectPage?id={Project.Id}");
            }
        }

        private async Task OnUpdateProgress()
        {
            if (Project != null)
            {
                try
                {
                    // Show a prompt to enter new progress percentage
                    string result = await Application.Current.MainPage.DisplayPromptAsync(
                        "Update Progress",
                        "Enter new progress percentage (0-100):",
                        "Update",
                        "Cancel",
                        initialValue: Project.Progress.ToString(),
                        maxLength: 3,
                        keyboard: Keyboard.Numeric);

                    if (!string.IsNullOrEmpty(result) && double.TryParse(result, out double newProgress))
                    {
                        // Validate progress value
                        if (newProgress < 0 || newProgress > 100)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Invalid Input",
                                "Progress must be between 0 and 100.",
                                "OK");
                            return;
                        }

                        // Update progress in the database via service
                        bool updated = await _projectService.UpdateProjectProgressAsync(Project.Id, newProgress);

                        if (updated)
                        {
                            // Update local model
                            Project.Progress = newProgress;
                            OnPropertyChanged(nameof(Project));

                            await Application.Current.MainPage.DisplayAlert(
                                "Success",
                                "Project progress updated successfully.",
                                "OK");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Error",
                                "Failed to update project progress.",
                                "OK");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating progress: {ex.Message}");

                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "An error occurred while updating progress.",
                        "OK");
                }
            }
        }
    }
}