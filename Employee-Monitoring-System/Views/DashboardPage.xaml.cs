namespace Employee_Monitoring_System.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        LoadDashboard();
    }

    private void LoadDashboard()
    {
        string userRole = GetUserRole(); // Fetch role from session or API

        if (userRole == "Admin")
        {
            AdminView.IsVisible = true;
        }
        else if (userRole == "TeamLeader")
        {
            TeamLeaderView.IsVisible = true;
        }
        else
        {
            EmployeeView.IsVisible = true;
        }
    }

    private string GetUserRole()
    {
        // Example: Get role from user session
        return Preferences.Get("UserRole", "Employee");
    }
}
