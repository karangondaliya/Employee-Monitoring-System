using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Employee_Monitoring_System.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Employee_Monitoring_System.Views
{
    public partial class AdminSettingsPage : ContentPage, IQueryAttributable
    {
        private readonly HttpClient _httpClient;
        private bool _isInitializing = true;

        public ObservableCollection<AppSettings> AllSettings { get; set; } = new ObservableCollection<AppSettings>();
        public bool IsTimerModeSelected { get; set; }
        public bool IsZeroClickModeSelected { get; set; }
        public double ScreenshotInterval { get; set; } = 10;
        public List<string> RetentionOptions { get; set; } = new List<string>
        {
            "30 days", "60 days", "90 days", "180 days", "365 days"
        };
        public string SelectedRetention { get; set; } = "90 days";

        public AdminSettingsPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            string userRole = Preferences.Get("UserRole", "Employee");
            if (userRole != "Admin")
            {
                await DisplayAlert("Access Denied", "You need admin privileges to access this page.", "OK");
                await Shell.Current.GoToAsync("//DashboardPage");
                return;
            }

            await LoadAppSettings();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Handle any query parameters if navigated with parameters
        }

        private async Task LoadAppSettings()
        {
            try
            {
                _isInitializing = true;
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "Authentication token is missing. Please log in again.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetFromJsonAsync<List<AppSettings>>("AppSettings");

                if (response != null && response.Count > 0)
                {
                    AllSettings.Clear();
                    foreach (var setting in response)
                    {
                        AllSettings.Add(setting);

                        // Handle tracking mode specifically
                        if (setting.SettingKey == "TrackingMode")
                        {
                            IsTimerModeSelected = setting.SettingValue == "Timer";
                            IsZeroClickModeSelected = setting.SettingValue == "ZeroClick";

                            OnPropertyChanged(nameof(IsTimerModeSelected));
                            OnPropertyChanged(nameof(IsZeroClickModeSelected));
                        }
                        else if (setting.SettingKey == "ScreenshotInterval")
                        {
                            if (double.TryParse(setting.SettingValue, out double interval))
                            {
                                ScreenshotInterval = interval;
                                OnPropertyChanged(nameof(ScreenshotInterval));
                            }
                        }
                        else if (setting.SettingKey == "DataRetention")
                        {
                            SelectedRetention = setting.SettingValue;
                            OnPropertyChanged(nameof(SelectedRetention));
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Info", "No app settings found.", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                await DisplayAlert("Error", $"Failed to fetch app settings: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                _isInitializing = false;
            }
        }

        private void OnTrackingModeChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_isInitializing) return;

            var radio = sender as RadioButton;
            if (radio == null || !e.Value) return;

            if (radio == TimerModeRadio)
            {
                IsTimerModeSelected = true;
                IsZeroClickModeSelected = false;
            }
            else if (radio == ZeroClickModeRadio)
            {
                IsTimerModeSelected = false;
                IsZeroClickModeSelected = true;
            }

            OnPropertyChanged(nameof(IsTimerModeSelected));
            OnPropertyChanged(nameof(IsZeroClickModeSelected));
        }

        private void OnIntervalChanged(object sender, ValueChangedEventArgs e)
        {
            if (_isInitializing) return;

            ScreenshotInterval = Math.Round(e.NewValue);
            OnPropertyChanged(nameof(ScreenshotInterval));

            // Update the setting in the collection
            var setting = AllSettings.FirstOrDefault(s => s.SettingKey == "ScreenshotInterval");
            if (setting != null)
            {
                setting.SettingValue = ScreenshotInterval.ToString();
            }
        }

        private void OnRetentionChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;

            // Update the setting in the collection
            var setting = AllSettings.FirstOrDefault(s => s.SettingKey == "DataRetention");
            if (setting != null)
            {
                setting.SettingValue = SelectedRetention;
            }
        }

        private void OnSettingValueChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;

            var entry = sender as Entry;
            if (entry == null) return;

            string key = entry.ClassId;
            string value = entry.Text;

            var setting = AllSettings.FirstOrDefault(s => s.SettingKey == key);
            if (setting != null)
            {
                setting.SettingValue = value;
            }
        }

        private async void OnAddSettingClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewSettingKeyEntry.Text) || string.IsNullOrWhiteSpace(NewSettingValueEntry.Text))
            {
                await DisplayAlert("Validation Error", "Both Setting Key and Value are required", "OK");
                return;
            }

            // Check if setting already exists
            if (AllSettings.Any(s => s.SettingKey == NewSettingKeyEntry.Text))
            {
                bool update = await DisplayAlert("Setting Exists",
                    "This setting already exists. Would you like to update it?", "Yes", "No");

                if (update)
                {
                    var existingSetting = AllSettings.First(s => s.SettingKey == NewSettingKeyEntry.Text);
                    existingSetting.SettingValue = NewSettingValueEntry.Text;
                }
                else
                {
                    return;
                }
            }
            else
            {
                // Add new setting to the collection
                AllSettings.Add(new AppSettings
                {
                    SettingKey = NewSettingKeyEntry.Text,
                    SettingValue = NewSettingValueEntry.Text
                });
            }

            // Clear inputs
            NewSettingKeyEntry.Text = string.Empty;
            NewSettingValueEntry.Text = string.Empty;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "Authentication token is missing. Please log in again.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Update or create the tracking mode setting
                await UpdateSetting("TrackingMode", IsTimerModeSelected ? "Timer" : "ZeroClick");

                // Update or create the screenshot interval setting
                await UpdateSetting("ScreenshotInterval", ScreenshotInterval.ToString());

                // Update or create the data retention setting
                await UpdateSetting("DataRetention", SelectedRetention);

                // Loop through all settings and save them
                foreach (var setting in AllSettings)
                {
                    if (setting.SettingKey != "TrackingMode" &&
                        setting.SettingKey != "ScreenshotInterval" &&
                        setting.SettingKey != "DataRetention")
                    {
                        await UpdateSetting(setting.SettingKey, setting.SettingValue);
                    }
                }

                await DisplayAlert("Success", "Settings saved successfully!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async Task UpdateSetting(string key, string value)
        {
            var setting = AllSettings.FirstOrDefault(s => s.SettingKey == key);

            if (setting != null)
            {
                // Setting exists, update it
                setting.SettingValue = value;
                var response = await _httpClient.PutAsJsonAsync($"AppSettings/{key}", setting);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to update setting {key}: {response.StatusCode}");
                }
            }
            else
            {
                // Setting doesn't exist, create it
                var newSetting = new AppSettings
                {
                    SettingKey = key,
                    SettingValue = value
                };

                var response = await _httpClient.PostAsJsonAsync("AppSettings", newSetting);

                if (response.IsSuccessStatusCode)
                {
                    AllSettings.Add(newSetting);
                }
                else
                {
                    throw new Exception($"Failed to create setting {key}: {response.StatusCode}");
                }
            }
        }
    }
}