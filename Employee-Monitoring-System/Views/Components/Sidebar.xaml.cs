namespace Employee_Monitoring_System.Views.Components;

public partial class Sidebar : ContentView
{
    public Sidebar()
    {
        InitializeComponent();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///LoginPage");

    }
}
