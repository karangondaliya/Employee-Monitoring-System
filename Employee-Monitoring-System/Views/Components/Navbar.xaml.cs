namespace Employee_Monitoring_System.Views.Components;

public partial class Navbar : ContentView
{
	public Navbar()
	{
		InitializeComponent();
	}

    private void OnDarkModeToggled(object sender, EventArgs e)
    {
        
        if (Application.Current.UserAppTheme == AppTheme.Dark)
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            DarkModeLabel.Text = "Light Mode";
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            DarkModeLabel.Text = "Dark Mode";
        }

    }
}