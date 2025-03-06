using System.Windows.Input;
using Employee_Monitoring_System.Views;

namespace Employee_Monitoring_System;

public class MainPageViewModel
{
    public ICommand NavigateToLoginCommand { get; }

    public MainPageViewModel()
    {
        NavigateToLoginCommand = new Command(async () => await NavigateToLogin());
    }

    private async Task NavigateToLogin()
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }

    public string Greeting => "Hello, .NET MAUI!";
}
