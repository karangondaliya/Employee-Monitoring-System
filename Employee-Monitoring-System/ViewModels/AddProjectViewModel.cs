using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using Employee_Monitoring_System.Models;
using Microsoft.Maui.Storage;

namespace Employee_Monitoring_System.ViewModels
{
    public class AddProjectViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;

        // Form fields
        private string _title;
        private string _description;
        private DateTime _startDate = DateTime.Now;
        private DateTime _endDate = DateTime.Now.AddDays(30);
        private string _selectedStatus = "Planning";
        private ObservableCollection<SelectableTeamMember> _teamMembers;
        private ObservableCollection<SelectableTeamMember> _selectedTeamMembers = new();

        // Validation errors
        private string _titleError;
        private string _descriptionError;
        private string _startDateError;
        private string _endDateError;
        private string _errorMessage;
        private bool _hasError;
        private bool _hasTitleError;
        private bool _hasDescriptionError;
        private bool _hasStartDateError;
        private bool _hasEndDateError;

        // Options for dropdowns
        private ObservableCollection<string> _statusOptions;

        public AddProjectViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7227/api/");

            // Initialize commands
            SaveProjectCommand = new Command(async () => await SaveProjectAsync());
            CancelCommand = new Command(async () => await CancelAsync());
            LoadTeamMembersCommand = new Command(async () => await LoadTeamMembersAsync());

            // Initialize collections
            TeamMembers = new ObservableCollection<SelectableTeamMember>();
            SelectedTeamMembers = new ObservableCollection<SelectableTeamMember>();
            StatusOptions = new ObservableCollection<string> { "Planning", "In Progress", "Completed" };

            // Load team members when view model is created
            MainThread.BeginInvokeOnMainThread(async () => await LoadTeamMembersAsync());
        }

        #region Properties

        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    ValidateTitle();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    ValidateDescription();
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    ValidateStartDate();
                    ValidateEndDate(); // Revalidate end date when start date changes
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    ValidateEndDate();
                }
            }
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public ObservableCollection<SelectableTeamMember> TeamMembers
        {
            get => _teamMembers;
            set => SetProperty(ref _teamMembers, value);
        }

        public ObservableCollection<SelectableTeamMember> SelectedTeamMembers
        {
            get => _selectedTeamMembers;
            set => SetProperty(ref _selectedTeamMembers, value);
        }

        // Validation properties
        public string TitleError
        {
            get => _titleError;
            set => SetProperty(ref _titleError, value);
        }

        public string DescriptionError
        {
            get => _descriptionError;
            set => SetProperty(ref _descriptionError, value);
        }

        public string StartDateError
        {
            get => _startDateError;
            set => SetProperty(ref _startDateError, value);
        }

        public string EndDateError
        {
            get => _endDateError;
            set => SetProperty(ref _endDateError, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    HasError = !string.IsNullOrEmpty(value);
                }
            }
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public bool HasTitleError
        {
            get => _hasTitleError;
            set => SetProperty(ref _hasTitleError, value);
        }

        public bool HasDescriptionError
        {
            get => _hasDescriptionError;
            set => SetProperty(ref _hasDescriptionError, value);
        }

        public bool HasStartDateError
        {
            get => _hasStartDateError;
            set => SetProperty(ref _hasStartDateError, value);
        }

        public bool HasEndDateError
        {
            get => _hasEndDateError;
            set => SetProperty(ref _hasEndDateError, value);
        }

        // Collections
        public ObservableCollection<string> StatusOptions
        {
            get => _statusOptions;
            set => SetProperty(ref _statusOptions, value);
        }

        #endregion

        #region Commands

        public ICommand SaveProjectCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand LoadTeamMembersCommand { get; }

        #endregion

        #region Methods

        private async Task LoadTeamMembersAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                // Get the authentication token from SecureStorage
                string token = await SecureStorage.Default.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Authentication Error",
                        "Authentication token is missing. Please log in again.",
                        "OK");
                    return;
                }

                // Set the Authorization header with the Bearer token
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Call the API to get team members
                var response = await _httpClient.GetAsync("Users");

                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<UserModel>>();

                    if (users != null)
                    {
                        TeamMembers.Clear();
                        foreach (var user in users)
                        {
                            TeamMembers.Add(new SelectableTeamMember
                            {
                                Id = user.UserId,
                                Name = user.Name,
                                Email = user.Email,
                                IsSelected = false
                            });
                        }
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        $"Failed to load team members. Status: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Failed to load team members: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // Clear error message
            ErrorMessage = string.Empty;

            // Validate title
            if (!ValidateTitle())
                isValid = false;

            // Validate description
            if (!ValidateDescription())
                isValid = false;

            // Validate start date
            if (!ValidateStartDate())
                isValid = false;

            // Validate end date
            if (!ValidateEndDate())
                isValid = false;

            // Check if at least one team member is selected
            var selectedMembers = TeamMembers.Where(t => t.IsSelected).ToList();
            if (selectedMembers.Count == 0)
            {
                ErrorMessage = "Please select at least one team member for the project";
                isValid = false;
            }

            return isValid;
        }

        private bool ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Project title is required";
                HasTitleError = true;
                return false;
            }

            if (Title.Length > 100)
            {
                TitleError = "Project title cannot exceed 100 characters";
                HasTitleError = true;
                return false;
            }

            TitleError = string.Empty;
            HasTitleError = false;
            return true;
        }

        private bool ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Project description is required";
                HasDescriptionError = true;
                return false;
            }

            DescriptionError = string.Empty;
            HasDescriptionError = false;
            return true;
        }

        private bool ValidateStartDate()
        {
            if (StartDate < DateTime.Now.Date)
            {
                StartDateError = "Start date cannot be in the past";
                HasStartDateError = true;
                return false;
            }

            StartDateError = string.Empty;
            HasStartDateError = false;
            return true;
        }

        private bool ValidateEndDate()
        {
            if (EndDate < DateTime.Now.Date)
            {
                EndDateError = "End date cannot be in the past";
                HasEndDateError = true;
                return false;
            }

            if (EndDate < StartDate)
            {
                EndDateError = "End date must be after start date";
                HasEndDateError = true;
                return false;
            }

            EndDateError = string.Empty;
            HasEndDateError = false;
            return true;
        }

        private async Task SaveProjectAsync()
        {
            if (IsBusy)
                return;

            if (!ValidateForm())
                return;

            IsBusy = true;

            try
            {
                // Get the authentication token from SecureStorage
                string token = await SecureStorage.Default.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Authentication Error",
                        "Authentication token is missing. Please log in again.",
                        "OK");
                    return;
                }

                // Set the Authorization header with the Bearer token
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Get selected team members
                var selectedTeamMembers = TeamMembers.Where(t => t.IsSelected)
                    .Select(t => new { Id = t.Id, Name = t.Name })
                    .ToList();

                // Create the project object
                var newProject = new
                {
                    ProjectName = Title,
                    Description = Description,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Status = SelectedStatus,
                    CompletionPercentage = 0,
                    TeamMembers = selectedTeamMembers
                };

                // Serialize the project object
                var content = new StringContent(
                    JsonSerializer.Serialize(newProject),
                    Encoding.UTF8,
                    "application/json");

                // Call the API to create the project
                var response = await _httpClient.PostAsync("Projects", content);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success",
                        "Project created successfully", "OK");

                    // Navigate back to the projects page
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error",
                        $"Failed to create project. Status: {response.StatusCode}\n{errorContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Failed to create project: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelAsync()
        {
            // Go back to the previous page
            await Shell.Current.GoToAsync("..");
        }

        #endregion
    }

    // Model class for selectable team members
    public class SelectableTeamMember : ObservableObject
    {
        private bool _isSelected;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}