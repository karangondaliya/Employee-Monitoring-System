namespace Employee_Monitoring_System.Views.Components
{
    public partial class Navbar : ContentView
    {
        public Navbar()
        {
            InitializeComponent();

            // Set the initial icon based on current theme
            UpdateThemeIcon();
        }

        private void UpdateThemeIcon()
        {
            // Set the appropriate icon based on current theme
            if (Application.Current.UserAppTheme == AppTheme.Dark)
            {
                ThemeToggleImage.Source = "moon.png";  // No file extension
            }
            else
            {
                ThemeToggleImage.Source = "sun.png";  // No file extension
            }
        }

        private void OnThemeIconTapped(object sender, EventArgs e)
        {
            // Toggle the theme
            if (Application.Current.UserAppTheme == AppTheme.Dark)
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }
            else
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
            }

            // Update the icon
            UpdateThemeIcon();
        }

        private async void OnProfileImageTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(UserProfilePage));
        }

        private async void OnNotificationIconTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NotificationsPage());
        }
    }
}