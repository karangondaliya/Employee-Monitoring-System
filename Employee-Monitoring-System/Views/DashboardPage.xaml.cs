using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using Newtonsoft.Json;
using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.Services;
using Employee_Monitoring_System.Views.Components;
using Microsoft.Maui.Controls.Compatibility; // Import for CardComponent

namespace Employee_Monitoring_System.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private bool _isTrackingActive = false;
        private ScreenshotService _screenshotService;

        public bool IsZeroClickMode { get; set; }
        public bool IsTimerMode { get; set; }
        public bool IsStartButtonVisible { get; set; }

        public DashboardPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
            _screenshotService = new ScreenshotService();
            LoadDashboardDataAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await UpdateTrackingModeAsync();
        }

        private async Task UpdateTrackingModeAsync()
        {
            IsZeroClickMode = false;
            IsTimerMode = false;
            IsStartButtonVisible = false;

            var appSettings = await FetchAppSettings();

            if (appSettings?.SettingValue == "ZeroClick")
            {
                IsZeroClickMode = true;
                _screenshotService.StartCapturingAsync(); // Auto-start
            }
            else if (appSettings?.SettingValue == "Timer")
            {
                IsTimerMode = true;
                IsStartButtonVisible = true;
            }

            OnPropertyChanged(nameof(IsZeroClickMode));
            OnPropertyChanged(nameof(IsTimerMode));
            OnPropertyChanged(nameof(IsStartButtonVisible));
        }

        public async Task<AppSettings> FetchAppSettings()
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "Authentication token is missing. Please log in again.", "OK");
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string id = "TrackingMode";

                var response = await _httpClient.GetFromJsonAsync<AppSettings>($"AppSettings/{id}");

                if (response == null)
                {
                    await DisplayAlert("Error", "AppSettings not found.", "OK");
                    return null;
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                await DisplayAlert("Error", $"Failed to fetch AppSettings: {ex.Message}", "OK");
                return null;
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                LoadingIndicator.IsVisible = true;

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

                string userRole = Preferences.Get("UserRole", "Employee");
                DashboardTitle.Text = $"{userRole} Dashboard";

                AdminView.IsVisible = false;
                TeamLeaderView.IsVisible = false;
                EmployeeView.IsVisible = false;

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
            AdminView.Children.Clear(); // Clear previous data

            // Add cards dynamically
            AdminView.Children.Add(new CardComponent
            {
                Title = "Total Employees",
                Value = $"{employees?.Count ?? 0}",
                Icon = "group.png", // Replace with your actual image file
                AdditionalText = $"+5 from last week"
            });

            AdminView.Children.Add(new CardComponent
            {
                Title = "Total Projects",
                Value = $"{projects?.Count ?? 0}",
                Icon = "project.png", // Replace with your actual image file
                AdditionalText = $"+2 from last week"
            });

            AdminView.Children.Add(new CardComponent
            {
                Title = "Total Tasks",
                Value = $"{tasks?.Count ?? 0}",
                Icon = "clipboard.png", // Replace with your actual image file
                AdditionalText = $"-1 from last week"
            });

            AdminView.Children.Add(new CardComponent
            {
                Title = "Total Leaves",
                Value = $"{leaves?.Count ?? 0}",
                Icon = "calendar.png", // Replace with your actual image file
                AdditionalText = $"+1 from last week"
            });
            if (IsZeroClickMode)
            {
                AddZeroClickModeCard(AdminViewZeroClick);
            }
        }

        private async Task LoadTeamLeaderData()
        {
            var projects = await _httpClient.GetFromJsonAsync<List<object>>("Projects");
            var tasks = await _httpClient.GetFromJsonAsync<List<object>>("_Task");

            TeamLeaderView.IsVisible = true;
            TeamLeaderView.Children.Clear(); // Clear previous data

            // Add cards dynamically
            TeamLeaderView.Children.Add(new CardComponent
            {
                Title = "Total Projects",
                Value = $"{projects?.Count ?? 0}",
                Icon = "project.png",
                AdditionalText = $"+2 from last week"
            });

            TeamLeaderView.Children.Add(new CardComponent
            {
                Title = "Pending Tasks",
                Value = $"{tasks?.Count ?? 0}",
                Icon = "clipboard.png",
                AdditionalText = $"+3 from last week"
            });
            if (IsZeroClickMode)
            {
                AddZeroClickModeCard(TeamLeaderViewZeroClick);
            }
        }

        private async Task LoadEmployeeData()
        {
            var tasks = await _httpClient.GetFromJsonAsync<List<object>>("_Task");
            var leaves = await _httpClient.GetFromJsonAsync<List<object>>("LeaveRequests");

            EmployeeView.IsVisible = true;
            EmployeeView.Children.Clear(); // Clear previous data

            // Add cards dynamically
            EmployeeView.Children.Add(new CardComponent
            {
                Title = "Assigned Tasks",
                Value = $"{tasks?.Count ?? 0}",
                Icon = "clipboard.png",
                AdditionalText = $"+4 from last week"
            });

            EmployeeView.Children.Add(new CardComponent
            {
                Title = "Leaves Taken",
                Value = $"{leaves?.Count ?? 0}",
                Icon = "calendar.png",
                AdditionalText = $"+1 from last week"
            });
            if (IsZeroClickMode)
            {
                AddZeroClickModeCard(EmployeeViewZeroClick);
            }
        }

        private async void StartButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            if (!_isTrackingActive)
            {
                await _screenshotService.StartCapturingAsync();
                button.Text = "Stop Tracking";
                button.ImageSource = "pause_icon_white.png";
                button.BackgroundColor = Colors.Red;
                _isTrackingActive = true;
            }
            else
            {
                _screenshotService.StopCapturing();
                button.Text = "Start Tracking";
                button.ImageSource = "play_icon_white.png";
                button.BackgroundColor = Color.FromArgb("#1DB954");
                _isTrackingActive = false;
            }
        }

        private void AddZeroClickModeCard(VerticalStackLayout targetView)
        {
            if (IsZeroClickMode)
            {
                targetView.Children.Add(new CardComponent
                {
                    Title = "Zero-Click Mode",
                    Value = "Active",
                    Icon = "zero_click_icon.png", // Add a new icon file for this if needed
                    AdditionalText = "Screenshots are being captured automatically."
                });
            }
        }


    }
}