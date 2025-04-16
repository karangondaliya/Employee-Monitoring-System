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
        Routing.RegisterRoute(nameof(UserProfilePage), typeof(UserProfilePage));
        Routing.RegisterRoute(nameof(UpdatePasswordPage), typeof(UpdatePasswordPage));
        Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
        Routing.RegisterRoute(nameof(LeaveRequestPage), typeof(LeaveRequestPage));
        Routing.RegisterRoute(nameof(AddLeaveRequestPage), typeof(AddLeaveRequestPage));
        Routing.RegisterRoute(nameof(ProjectsPage), typeof(ProjectsPage));
    }
}
