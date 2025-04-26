using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.Services;

namespace Employee_Monitoring_System.ViewModels
{
    public class ProjectsViewModel : BaseViewModel
    {
        private readonly ProjectService _projectService;
        private bool _isLoading;
        private string _searchQuery;
        private string _selectedStatus;
        private ICommand _refreshCommand;
        private ICommand _newProjectCommand;
        private ICommand _viewDetailsCommand;

        public ObservableCollection<Project> Projects { get; } = new();
        public ObservableCollection<string> Statuses { get; } = new()
        {
            "All Statuses",
            "Planning",
            "In Progress",
            "Completed"
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
                    LoadProjectsAsync().ConfigureAwait(false);
                }
            }
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    LoadProjectsAsync().ConfigureAwait(false);
                }
            }
        }

        public bool IsAdmin { get; set; }

        // Update command properties to include setters
        public ICommand RefreshCommand
        {
            get => _refreshCommand;
            set => SetProperty(ref _refreshCommand, value);
        }

        public ICommand NewProjectCommand
        {
            get => _newProjectCommand;
            set => SetProperty(ref _newProjectCommand, value);
        }

        public ICommand ViewDetailsCommand
        {
            get => _viewDetailsCommand;
            set => SetProperty(ref _viewDetailsCommand, value);
        }

        // Public parameterless constructor for XAML
        public ProjectsViewModel()
        {
            _projectService = new ProjectService(); // Create service instance directly for XAML initialization
            Initialize();
        }

        // Constructor with dependency injection
        public ProjectsViewModel(ProjectService projectService)
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
            RefreshCommand = new Command(async () => await LoadProjectsAsync());
            NewProjectCommand = new Command(async () => await OnNewProject());
            ViewDetailsCommand = new Command<Project>(async (project) => await OnViewDetails(project));

            // Initial load
            LoadProjectsAsync().ConfigureAwait(false);
        }

        private async Task LoadProjectsAsync()
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

                var status = SelectedStatus == "All Statuses" ? null : SelectedStatus;
                var projects = await _projectService.GetProjectsAsync(SearchQuery, status);

                // Debug output to confirm data is received
                System.Diagnostics.Debug.WriteLine($"Received {projects?.Count ?? 0} projects from service");
                foreach (var project in projects)
                {
                    System.Diagnostics.Debug.WriteLine($"Project: {project.Id} - {project.Title} - Status: {project.Status}");
                    System.Diagnostics.Debug.WriteLine($"  Description: {project.Description}");
                    System.Diagnostics.Debug.WriteLine($"  Progress: {project.Progress}, Team: {project.Team}");
                    System.Diagnostics.Debug.WriteLine($"  Deadline: {project.Deadline}");
                }

                // Update UI on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Projects.Clear();
                    if (projects != null)
                    {
                        foreach (var project in projects)
                        {
                            Projects.Add(project);
                        }
                    }
                    // Force UI refresh
                    OnPropertyChanged(nameof(Projects));
                    System.Diagnostics.Debug.WriteLine($"Projects collection updated with {Projects.Count} items");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading projects: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                await Application.Current.MainPage.DisplayAlert("Error",
                    "Unable to load projects. Please try again later.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnNewProject()
        {
            try
            {
                // Navigate to new project page or show modal
                await Application.Current.MainPage.DisplayAlert("New Project",
                    "Navigate to new project form", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Unable to create new project.", "OK");
            }
        }

        private async Task OnViewDetails(Project project)
        {
            if (project != null)
            {
                try
                {
                    // Navigate to project details page
                    await Application.Current.MainPage.DisplayAlert("Project Details",
                        $"Viewing details for {project.Title}", "OK");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Unable to load project details.", "OK");
                }
            }
        }
    }
}