namespace Employee_Monitoring_System.Views;

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
}
