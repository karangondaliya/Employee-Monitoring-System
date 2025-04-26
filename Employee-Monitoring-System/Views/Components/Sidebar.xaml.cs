using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Views.Components
{
    public partial class Sidebar : ContentView
    {
        public Sidebar()
        {
            InitializeComponent();

            // Force refresh sidebar if needed
            bool needsRefresh = Preferences.Get("ForceRefreshSidebar", false);
            if (needsRefresh)
            {
                System.Diagnostics.Debug.WriteLine("Initializing sidebar with refresh flag set");
                Preferences.Set("ForceRefreshSidebar", false);
            }

            BindingContext = SidebarViewModel.Create();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                // Clean up
                Preferences.Remove("UserRole");
                Preferences.Remove("UserName");
                Preferences.Remove("JobTitle");
                Preferences.Set("ForceRefreshSidebar", true);

                // Clear secure storage
                SecureStorage.Remove("auth_token");
                SecureStorage.Remove("UserId");

                System.Diagnostics.Debug.WriteLine("User logged out");

                // Try Shell navigation first
                try
                {
                    await Shell.Current.GoToAsync("//LoginPage");
                }
                catch
                {
                    // Fallback to direct page setting
                    Application.Current.MainPage = new NavigationPage(new Employee_Monitoring_System.Views.LoginPage());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during logout: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Logout Error",
                    "There was an error logging out. Please try again.",
                    "OK");
            }
        }

        private async void OnUserProfileTapped(object sender, EventArgs e)
        {
            try
            {
                // Navigate to user profile page
                System.Diagnostics.Debug.WriteLine("User profile tapped - navigating to profile page");
                await Shell.Current.GoToAsync("//UserProfilePage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to profile: {ex.Message}");
                // Show a user-friendly message
                await Application.Current.MainPage.DisplayAlert("Navigation Error",
                    "Unable to navigate to profile page. The route might not be registered.",
                    "OK");
            }
        }
    }
}