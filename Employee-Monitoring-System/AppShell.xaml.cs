using Employee_Monitoring_System;
using Employee_Monitoring_System.Views;

namespace Employee_Monitoring_System;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
    }
}
