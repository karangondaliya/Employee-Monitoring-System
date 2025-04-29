using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginPageViewModel();
        }

        private async void OnUpdatePasswordTapped(object sender, EventArgs e)
        {
            // TODO: Implement forgot password logic
            await Navigation.PushAsync(new UpdatePasswordPage());
        }
        private void LoginButton_Pressed(object sender, EventArgs e)
        {
            // Scale down slightly when pressed
            LoginButton.ScaleTo(0.95, 50, Easing.CubicOut);
            LoginButton.BackgroundColor = Color.FromArgb("#3a8f40");
        }
        private void LoginButton_Released(object sender, EventArgs e)
        {
            // Return to normal when released
            LoginButton.ScaleTo(1, 50, Easing.CubicOut);
            LoginButton.BackgroundColor = Color.FromArgb("#4CAF50");
        }
    }

}