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
    public class TasksPageViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<TaskModel> _tasks;
        private ObservableCollection<TaskModel> _filteredTasks;
        private string _searchText;
        private string _filterMode = "All";
        private Color _allTasksFilterColor = Colors.LightBlue;
        private Color _inProgressFilterColor = Colors.Transparent;
        private Color _completedFilterColor = Colors.Transparent;

        public TasksPageViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7227/api/"); // Update with your API URL

            Tasks = new ObservableCollection<TaskModel>();
            FilteredTasks = new ObservableCollection<TaskModel>();

            LoadTasksCommand = new Command(async () => await LoadTasksAsync());
            SearchCommand = new Command(ExecuteSearch);
            FilterCommand = new Command<string>(ExecuteFilter);
            TaskSelectedCommand = new Command<TaskModel>(OnTaskSelected);
            AddTaskCommand = new Command(async () => await OnAddTaskAsync());

            // Load tasks when the ViewModel is created
            MainThread.BeginInvokeOnMainThread(async () => await LoadTasksAsync());
        }

        public ObservableCollection<TaskModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public ObservableCollection<TaskModel> FilteredTasks
        {
            get => _filteredTasks;
            set => SetProperty(ref _filteredTasks, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ExecuteSearch();
                }
            }
        }

        public Color AllTasksFilterColor
        {
            get => _allTasksFilterColor;
            set => SetProperty(ref _allTasksFilterColor, value);
        }

        public Color InProgressFilterColor
        {
            get => _inProgressFilterColor;
            set => SetProperty(ref _inProgressFilterColor, value);
        }

        public Color CompletedFilterColor
        {
            get => _completedFilterColor;
            set => SetProperty(ref _completedFilterColor, value);
        }

        public ICommand LoadTasksCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand TaskSelectedCommand { get; }
        public ICommand AddTaskCommand { get; }

        private async Task LoadTasksAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Tasks.Clear();

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

                string endpoint = "_Task";
                System.Diagnostics.Debug.WriteLine($"Calling API endpoint: {_httpClient.BaseAddress}{endpoint}");

                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API Response: {jsonString}");

                    // Deserialize to our API-matching model
                    var apiTasks = await response.Content.ReadFromJsonAsync<List<ApiTaskModel>>();

                    if (apiTasks != null)
                    {
                        foreach (var apiTask in apiTasks)
                        {
                            try
                            {
                                // Convert API model to our UI model
                                var task = new TaskModel
                                {
                                    Id = apiTask.TaskId,
                                    Title = apiTask.TaskName,
                                    Description = apiTask.Description ?? "",
                                    CreatedDate = apiTask.StartDate,
                                    DueDate = apiTask.EndDate ?? DateTime.Now.AddDays(7),
                                    Status = ParseTaskStatus(apiTask.Status),
                                    Priority = ParseTaskPriority(apiTask.Priority),
                                    AssignedTo = apiTask.AssignedUsers != null && apiTask.AssignedUsers.Any()
                                        ? string.Join(", ", apiTask.AssignedUsers.Select(u => u.Name))
                                        : "Not Assigned",
                                    AssignedBy = "" // Not available in API
                                };

                                // Add the task to the collection
                                Tasks.Add(task);
                                System.Diagnostics.Debug.WriteLine($"Added task: {task.Id} - {task.Title}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error mapping task: {ex.Message}");
                            }
                        }
                    }

                    // Apply current filter and search
                    ApplyFilterAndSearch();

                    System.Diagnostics.Debug.WriteLine($"Total tasks: {Tasks.Count}, Filtered tasks: {FilteredTasks.Count}");
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode}, Content: {errorContent}");

                    await Application.Current.MainPage.DisplayAlert("Error",
                        $"Failed to load tasks. Status: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Failed to load tasks: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecuteSearch()
        {
            ApplyFilterAndSearch();
        }

        private void ExecuteFilter(string filterMode)
        {
            _filterMode = filterMode;

            // Update filter button colors
            AllTasksFilterColor = _filterMode == "All" ? Colors.LightBlue : Colors.Transparent;
            InProgressFilterColor = _filterMode == "InProgress" ? Colors.LightBlue : Colors.Transparent;
            CompletedFilterColor = _filterMode == "Completed" ? Colors.LightBlue : Colors.Transparent;

            ApplyFilterAndSearch();
        }

        private void ApplyFilterAndSearch()
        {
            // First apply the status filter
            IEnumerable<TaskModel> filtered = _filterMode switch
            {
                "InProgress" => Tasks.Where(t => t.Status == TaskStatus.InProgress),
                "Completed" => Tasks.Where(t => t.Status == TaskStatus.Completed),
                _ => Tasks // "All" filter or default
            };

            // Then apply search if needed
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                filtered = filtered.Where(t =>
                    t.Title.ToLower().Contains(searchLower) ||
                    t.Description.ToLower().Contains(searchLower) ||
                    t.AssignedTo.ToLower().Contains(searchLower));
            }

            // Update the filtered collection
            FilteredTasks.Clear();
            foreach (var task in filtered)
            {
                FilteredTasks.Add(task);
            }
        }

        private async void OnTaskSelected(TaskModel task)
        {
            if (task == null)
                return;

            // Navigate to task details page
            var parameters = new Dictionary<string, object>
            {
                { "TaskId", task.Id }
            };

            await Application.Current.MainPage.DisplayAlert("Coming Soon", "Task Details feature will be available in a future update.", "OK");
        }

        private async Task OnAddTaskAsync()
        {
            await Application.Current.MainPage.DisplayAlert("Coming Soon", "Add Tasks feature will be available in a future update.", "OK");
        }

        private TaskStatus ParseTaskStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return TaskStatus.NotStarted;

            return status.ToLower() switch
            {
                "in progress" => TaskStatus.InProgress,
                "completed" => TaskStatus.Completed,
                "cancelled" => TaskStatus.Cancelled,
                _ => TaskStatus.NotStarted
            };
        }

        private TaskPriority ParseTaskPriority(string priority)
        {
            if (string.IsNullOrEmpty(priority))
                return TaskPriority.Medium;

            return priority.ToLower() switch
            {
                "low" => TaskPriority.Low,
                "medium" => TaskPriority.Medium,
                "high" => TaskPriority.High,
                "critical" => TaskPriority.Critical,
                _ => TaskPriority.Medium
            };
        }
    }
}