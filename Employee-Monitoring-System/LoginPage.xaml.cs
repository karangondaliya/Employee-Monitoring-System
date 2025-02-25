using System.ComponentModel;

namespace Employee_Monitoring_System
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginPageViewModel();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // TODO: Implement login logic
            await Navigation.PushAsync(new DashboardPage());
        }

        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            // TODO: Implement forgot password logic
            await DisplayAlert("Forgot Password", "Forgot password tapped", "OK");
        }
    }
}
