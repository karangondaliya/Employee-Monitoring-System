using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.Generic;

namespace Employee_Monitoring_System.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public DashboardPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };

            LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // Show loading indicator
                LoadingIndicator.IsVisible = true;

                // Retrieve the token securely
                string token = await SecureStorage.GetAsync("auth_token");

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    await DisplayAlert("Error", "Authentication token is missing. Please log in again.", "OK");
                    return;
                }

                // Fetch user role from Preferences
                string userRole = Preferences.Get("UserRole", "Employee");
                DashboardTitle.Text = $"{userRole} Dashboard";

                // Fetch data based on role
                if (userRole == "Admin")
                {
                    await LoadAdminData();
                }
                else if (userRole == "TeamLeader")
                {
                    await LoadTeamLeaderData();
                }
                else
                {
                    await LoadEmployeeData();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }


        private async Task LoadAdminData()
        {
            var employees = await _httpClient.GetFromJsonAsync<List<object>>("Users");
            var projects = await _httpClient.GetFromJsonAsync<List<object>>("Projects");
            var tasks = await _httpClient.GetFromJsonAsync<List<object>>("_Task");
            var leaves = await _httpClient.GetFromJsonAsync<List<object>>("LeaveRequests");

            AdminView.IsVisible = true;
            TotalEmployeesLabel.Text = $"Total Employees: {employees?.Count ?? 0}";
            TotalProjectsLabel.Text = $"Total Projects: {projects?.Count ?? 0}";
            TotalTasksLabel.Text = $"Total Tasks: {tasks?.Count ?? 0}";
            LeavesTakenLabel.Text = $"Total Leaves: {leaves?.Count ?? 0}";
        }

        private async Task LoadTeamLeaderData()
        {
            var projects = await _httpClient.GetFromJsonAsync<List<object>>("Projects");
            var tasks = await _httpClient.GetFromJsonAsync<List<object>>("_Task");

            TeamLeaderView.IsVisible = true;
            TLTotalProjectsLabel.Text = $"Total Projects: {projects?.Count ?? 0}";
            PendingTasksLabel.Text = $"Pending Tasks: {tasks?.Count ?? 0}";
        }

        private async Task LoadEmployeeData()
        {
            var tasks = await _httpClient.GetFromJsonAsync<List<object>>("_Task");
            var leaves = await _httpClient.GetFromJsonAsync<List<object>>("LeaveRequests");

            EmployeeView.IsVisible = true;
            EmpTotalProjectsLabel.Text = $"Total Projects: {tasks?.Count ?? 0}";
            AssignedTasksLabel.Text = $"Assigned Tasks: {tasks?.Count ?? 0}";
            AttendanceLabel.Text = $"Attendance: Present"; // Modify this if you have an attendance API
            EmpLeavesTakenLabel.Text = $"Leaves Taken: {leaves?.Count ?? 0}";
        }
    }
}
