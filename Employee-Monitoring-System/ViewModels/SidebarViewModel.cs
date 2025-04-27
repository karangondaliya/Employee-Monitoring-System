using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Employee_Monitoring_System.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace Employee_Monitoring_System.ViewModels
{
    public class SidebarViewModel : INotifyPropertyChanged
    {
        // Static field to track the current sidebar role
        private static string _lastUsedRole = null;

        public static SidebarViewModel Create()
        {
            var viewModel = new SidebarViewModel();

            // Check if we need to refresh due to a role change
            string currentRole = Preferences.Get("UserRole", "Employee");
            bool forceRefresh = Preferences.Get("ForceRefreshSidebar", false);

            if (forceRefresh || _lastUsedRole != currentRole)
            {
                System.Diagnostics.Debug.WriteLine($"Role change detected: {_lastUsedRole} -> {currentRole} (Force: {forceRefresh})");
                _lastUsedRole = currentRole;
                Preferences.Set("ForceRefreshSidebar", false);
            }

            return viewModel;
        }

        private string _activePage;
        public string ActivePage
        {
            get => _activePage;
            set
            {
                _activePage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SidebarItem> SidebarItems { get; private set; }
        public ICommand NavigateCommand { get; }
        private readonly HttpClient _httpClient;

        public SidebarViewModel()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
            SidebarItems = new ObservableCollection<SidebarItem>();
            NavigateCommand = new Command<SidebarItem>(NavigateToPage);

            // Get current role directly from preferences
            string currentRole = Preferences.Get("UserRole", "Employee");
            System.Diagnostics.Debug.WriteLine($"Initializing sidebar with role: {currentRole}");

            // Load sidebar with current role
            LoadSidebarItems(currentRole);

            // Load user data asynchronously
            Task.Run(async () => await LoadUserDataAsync());
        }

        private string _userName = "Loading...";
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        private string _userRole = "Loading...";
        public string UserRole
        {
            get => _userRole;
            set
            {
                _userRole = value;
                OnPropertyChanged();
            }
        }

        private ImageSource _userProfileImage;
        public ImageSource UserProfileImage
        {
            get => _userProfileImage;
            set
            {
                _userProfileImage = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public void LoadSidebarItems(string userRole = null)
        {
            SidebarItems.Clear();

            // Use the provided role or get from preferences
            userRole = userRole ?? Preferences.Get("UserRole", "Employee");
            System.Diagnostics.Debug.WriteLine($"Loading sidebar items for role: {userRole}");

            // Store the current role
            _lastUsedRole = userRole;

            // Common item for all roles
            SidebarItems.Add(new SidebarItem { Title = "Dashboard", Icon = "dashboard.png", NavigationTarget = "//DashboardPage" });

            // Add role-specific menu items
            if (userRole == "Admin")
            {
                System.Diagnostics.Debug.WriteLine("Adding ADMIN menu items");
                SidebarItems.Add(new SidebarItem { Title = "Manage Employees", Icon = "users.png", NavigationTarget = "//EmployeesPage" });
                SidebarItems.Add(new SidebarItem { Title = "Manage Projects", Icon = "briefcase.png", NavigationTarget = "//ProjectsPage" });
                //SidebarItems.Add(new SidebarItem { Title = "Manage Notifications", Icon = "notification_icon.png", NavigationTarget = "//ManageNotificationsPage" });
                //SidebarItems.Add(new SidebarItem { Title = "Manage Branches", Icon = "office.png", NavigationTarget = "//ManageBranchesPage" });
                //SidebarItems.Add(new SidebarItem { Title = "Settings", Icon = "settings.png", NavigationTarget = "//SettingsPage" });
                //SidebarItems.Add(new SidebarItem { Title = "View Screenshots", Icon = "landscape.png", NavigationTarget = "//ViewScreenshotsPage" });
                //SidebarItems.Add(new SidebarItem { Title = "Track Activity", Icon = "task.png", NavigationTarget = "//TrackActivityPage" });
                SidebarItems.Add(new SidebarItem { Title = "View Leaves", Icon = "calendar_white.png", NavigationTarget = "//LeaveRequestPage" });
            }
            else if (userRole == "TeamLead")
            {
                System.Diagnostics.Debug.WriteLine("Adding TEAM LEAD menu items");
                SidebarItems.Add(new SidebarItem { Title = "Manage Tasks", Icon = "to_do_list.png", NavigationTarget = "//TasksPage" });
                SidebarItems.Add(new SidebarItem { Title = "Manage Leaves", Icon = "calendar_white.png", NavigationTarget = "//LeaveRequestPage" });
                SidebarItems.Add(new SidebarItem { Title = "Projects", Icon = "briefcase.png", NavigationTarget = "//ProjectsPage" });
            }
            else if (userRole == "Employee")
            {
                System.Diagnostics.Debug.WriteLine("Adding EMPLOYEE menu items");
                SidebarItems.Add(new SidebarItem { Title = "My Tasks", Icon = "to_do_list.png", NavigationTarget = "//TasksPage" });
                SidebarItems.Add(new SidebarItem { Title = "My Leaves", Icon = "calendar_white.png", NavigationTarget = "//LeaveRequestPage" });
                SidebarItems.Add(new SidebarItem { Title = "My Projects", Icon = "briefcase.png", NavigationTarget = "//ProjectsPage" });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Unknown role: {userRole}, showing minimal sidebar");
            }

            OnPropertyChanged(nameof(SidebarItems));
            System.Diagnostics.Debug.WriteLine($"Sidebar loaded with {SidebarItems.Count} items for role: {userRole}");
        }

        private async void NavigateToPage(SidebarItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Title))
                return;

            try
            {
                ActivePage = item.Title;

                // Special handling for leaves page - recheck role
                if (item.Title.Contains("Leaves"))
                {
                    string currentRole = Preferences.Get("UserRole", "Employee");
                    System.Diagnostics.Debug.WriteLine($"Navigating to leaves page with role: {currentRole}");

                    // Always reload sidebar before navigating to a leaves page
                    LoadSidebarItems(currentRole);
                }

                // Use the explicit navigation target
                if (!string.IsNullOrEmpty(item.NavigationTarget))
                {
                    await Shell.Current.GoToAsync(item.NavigationTarget);
                }
                else
                {
                    string pageName = item.Title.Replace(" ", "").Replace("-", "") + "Page";
                    await Shell.Current.GoToAsync($"//{pageName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");

                await Application.Current.MainPage.DisplayAlert("Navigation Error",
                    $"Unable to navigate to {item.Title} page. The route might not be registered in AppShell.xaml.",
                    "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Async method to load user data from API
        public async Task LoadUserDataAsync()
        {
            try
            {
                IsLoading = true;

                // Get the authentication token
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("Auth token is missing");
                    SetDefaultUserValues();
                    return;
                }

                // Set the authorization header
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Get the user ID from secure storage
                string userId = await SecureStorage.GetAsync("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    System.Diagnostics.Debug.WriteLine("User ID is missing");
                    SetDefaultUserValues();
                    return;
                }

                // Fetch user data
                var response = await _httpClient.GetAsync($"Users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<User>();
                    if (user != null)
                    {
                        // Set user name
                        UserName = user.FullName;

                        // Update role in preferences if needed
                        if (!string.IsNullOrEmpty(user.Role))
                        {
                            string storedRole = Preferences.Get("UserRole", "");
                            if (user.Role != storedRole)
                            {
                                System.Diagnostics.Debug.WriteLine($"Role changed from API: {storedRole} -> {user.Role}");
                                Preferences.Set("UserRole", user.Role);

                                // Reload sidebar items with new role
                                LoadSidebarItems(user.Role);
                            }
                        }

                        // Set job title display
                        if (!string.IsNullOrEmpty(user.JobTitle))
                        {
                            UserRole = user.JobTitle;
                        }
                        else
                        {
                            UserRole = ConvertRoleToJobTitle(user.Role);
                        }

                        // Save values to preferences
                        Preferences.Set("UserName", UserName);
                        Preferences.Set("JobTitle", UserRole);

                        // Fetch profile image
                        await LoadProfileImageAsync(userId);
                    }
                    else
                    {
                        SetDefaultUserValues();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to get user data: {response.StatusCode}");
                    SetDefaultUserValues();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user data: {ex.Message}");
                SetDefaultUserValues();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProfileImageAsync(string userId)
        {
            try
            {
                // First check if we have the image cached
                string cachedImagePath = Path.Combine(FileSystem.CacheDirectory, $"profile_image_{userId}.png");

                // Check if the cached file exists and is recent (less than 1 day old)
                if (File.Exists(cachedImagePath) &&
                    (DateTime.Now - File.GetLastWriteTime(cachedImagePath)).TotalDays < 1)
                {
                    // Use cached image
                    UserProfileImage = ImageSource.FromFile(cachedImagePath);
                    return;
                }

                // Fetch the image from API
                var imageResponse = await _httpClient.GetAsync($"Users/{userId}/profile-image");
                if (imageResponse.IsSuccessStatusCode)
                {
                    var imageData = await imageResponse.Content.ReadAsByteArrayAsync();
                    if (imageData != null && imageData.Length > 0)
                    {
                        // Save to cache
                        await File.WriteAllBytesAsync(cachedImagePath, imageData);

                        // Set as profile image
                        UserProfileImage = ImageSource.FromStream(() => new MemoryStream(imageData));
                    }
                    else
                    {
                        SetDefaultProfileImage();
                    }
                }
                else
                {
                    SetDefaultProfileImage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile image: {ex.Message}");
                SetDefaultProfileImage();
            }
        }

        private void SetDefaultProfileImage()
        {
            // Set default profile image
            UserProfileImage = ImageSource.FromFile("person.png");
        }

        private void SetDefaultUserValues()
        {
            // Set default values when we can't load user data
            UserName = Preferences.Get("UserName", "User");
            UserRole = Preferences.Get("JobTitle", "Employee");
            SetDefaultProfileImage();
        }

        // Helper method to convert role to job title
        private string ConvertRoleToJobTitle(string role)
        {
            switch (role)
            {
                case "Admin":
                    return "System Administrator";
                case "TeamLeader":
                case "TeamLead":
                    return "Team Leader";
                case "Employee":
                    return "Software Developer";
                default:
                    return role;
            }
        }
    }
}