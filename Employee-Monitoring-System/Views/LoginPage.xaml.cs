namespace Employee_Monitoring_System.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DashboardPage());
    }
    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        // TODO: Implement forgot password logic
        await DisplayAlert("Forgot Password", "Forgot password tapped", "OK");
    }
}
