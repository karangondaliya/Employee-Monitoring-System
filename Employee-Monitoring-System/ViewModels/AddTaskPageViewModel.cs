using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using Employee_Monitoring_System.Models;
using TaskStatus = Employee_Monitoring_System.Models.TaskStatus;
using Microsoft.Maui.Storage;

namespace Employee_Monitoring_System.ViewModels
{
    public class AddTaskPageViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;

        // Form fields
        private string _title;
        private string _description;
        private DateTime _dueDate = DateTime.Now.AddDays(7);
        private DateTime _startDate = DateTime.Now;
        private string _selectedPriority = "Medium";
        private string _selectedStatus = "Not Started";
        private string _selectedTeamMember;
        private string _notes;

        // Validation errors
        private string _titleError;
        private string _descriptionError;
        private string _dueDateError;
        private string _startDateError;
        private string _errorMessage;
        private bool _hasError;
        private bool _hasTitleError;
        private bool _hasDescriptionError;
        private bool _hasDueDateError;
        private bool _hasStartDateError;

        // Options for dropdowns
        private ObservableCollection<string> _teamMembers;
        private ObservableCollection<string> _priorityOptions;
        private ObservableCollection<string> _statusOptions;

        public AddTaskPageViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7227/api/"); // Update with your API URL

            // Initialize commands
            SaveTaskCommand = new Command(async () => await SaveTaskAsync());
            CancelCommand = new Command(async () => await CancelAsync());
            LoadTeamMembersCommand = new Command(async () => await LoadTeamMembersAsync());

            // Initialize collections
            TeamMembers = new ObservableCollection<string>();
            PriorityOptions = new ObservableCollection<string> { "Low", "Medium", "High", "Critical" };
            StatusOptions = new ObservableCollection<string> { "Not Started", "In Progress", "Completed", "Cancelled" };
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

        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                if (SetProperty(ref _dueDate, value))
                {
                    ValidateDueDate();
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
                }
            }
        }

        public string SelectedPriority
        {
            get => _selectedPriority;
            set => SetProperty(ref _selectedPriority, value);
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public string SelectedTeamMember
        {
            get => _selectedTeamMember;
            set => SetProperty(ref _selectedTeamMember, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
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

        public string DueDateError
        {
            get => _dueDateError;
            set => SetProperty(ref _dueDateError, value);
        }

        public string StartDateError
        {
            get => _startDateError;
            set => SetProperty(ref _startDateError, value);
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

        public bool HasDueDateError
        {
            get => _hasDueDateError;
            set => SetProperty(ref _hasDueDateError, value);
        }

        public bool HasStartDateError
        {
            get => _hasStartDateError;
            set => SetProperty(ref _hasStartDateError, value);
        }

        // Collections
        public ObservableCollection<string> TeamMembers
        {
            get => _teamMembers;
            set => SetProperty(ref _teamMembers, value);
        }

        public ObservableCollection<string> PriorityOptions
        {
            get => _priorityOptions;
            set => SetProperty(ref _priorityOptions, value);
        }

        public ObservableCollection<string> StatusOptions
        {
            get => _statusOptions;
            set => SetProperty(ref _statusOptions, value);
        }

        #endregion

        #region Commands

        public ICommand SaveTaskCommand { get; }
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
                            TeamMembers.Add(user.Name);
                        }

                        // Set default team member if available
                        if (TeamMembers.Count > 0 && string.IsNullOrEmpty(SelectedTeamMember))
                        {
                            SelectedTeamMember = TeamMembers[0];
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

            // Validate due date
            if (!ValidateDueDate())
                isValid = false;

            // Validate start date
            if (!ValidateStartDate())
                isValid = false;

            // Validate team member
            if (string.IsNullOrEmpty(SelectedTeamMember))
            {
                ErrorMessage = "Please select a team member to assign the task";
                isValid = false;
            }

            return isValid;
        }

        private bool ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Task title is required";
                HasTitleError = true;
                return false;
            }

            if (Title.Length > 100)
            {
                TitleError = "Task title cannot exceed 100 characters";
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
                DescriptionError = "Task description is required";
                HasDescriptionError = true;
                return false;
            }

            DescriptionError = string.Empty;
            HasDescriptionError = false;
            return true;
        }

        private bool ValidateDueDate()
        {
            if (DueDate < DateTime.Now.Date)
            {
                DueDateError = "Due date cannot be in the past";
                HasDueDateError = true;
                return false;
            }

            if (StartDate > DueDate)
            {
                DueDateError = "Due date must be after start date";
                HasDueDateError = true;
                return false;
            }

            DueDateError = string.Empty;
            HasDueDateError = false;
            return true;
        }

        private bool ValidateStartDate()
        {
            if (StartDate > DueDate)
            {
                StartDateError = "Start date must be before due date";
                HasStartDateError = true;
                return false;
            }

            StartDateError = string.Empty;
            HasStartDateError = false;
            return true;
        }

        private async Task SaveTaskAsync()
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

                // Create the task object
                var newTask = new
                {
                    TaskName = Title,
                    Description = Description,
                    StartDate = StartDate,
                    EndDate = DueDate,
                    Status = SelectedStatus,
                    Priority = SelectedPriority,
                    AssignedUsers = new[] { new { Name = SelectedTeamMember } },
                    Notes = Notes
                };

                // Serialize the task object
                var content = new StringContent(
                    JsonSerializer.Serialize(newTask),
                    Encoding.UTF8,
                    "application/json");

                // Call the API to create the task
                var response = await _httpClient.PostAsync("_Task", content);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success",
                        "Task created successfully", "OK");

                    // Navigate back to the tasks page
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error",
                        $"Failed to create task. Status: {response.StatusCode}\n{errorContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Failed to create task: {ex.Message}", "OK");
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

    // This class is used to deserialize the user data from the API
    public class UserModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}