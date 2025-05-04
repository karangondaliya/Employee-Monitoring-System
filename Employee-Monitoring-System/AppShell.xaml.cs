using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Employee_Monitoring_System.ViewModels;
using Employee_Monitoring_System.Views.Components;
using Employee_Monitoring_System.Views;

namespace Employee_Monitoring_System
{
    public partial class AppShell : Shell
    {
        private Sidebar sidebarInstance;

        public AppShell()
        {
            InitializeComponent();

            // Register routes
            Routing.RegisterRoute("LoginPage", typeof(Views.LoginPage));
            Routing.RegisterRoute("DashboardPage", typeof(Views.DashboardPage));
            Routing.RegisterRoute("UserProfilePage", typeof(Views.UserProfilePage));
            Routing.RegisterRoute("UpdatePasswordPage", typeof(Views.UpdatePasswordPage));
            Routing.RegisterRoute("NotificationsPage", typeof(Views.NotificationsPage));
            Routing.RegisterRoute("LeaveRequestPage", typeof(Views.LeaveRequestPage));
            Routing.RegisterRoute("AddLeaveRequestPage", typeof(Views.AddLeaveRequestPage));
            Routing.RegisterRoute("ProjectsPage", typeof(Views.ProjectsPage));
            Routing.RegisterRoute("EditLeaveRequestPage", typeof(Views.EditLeaveRequestPage));
            Routing.RegisterRoute("TasksPage", typeof(Views.TasksPage));
            Routing.RegisterRoute("EmployeesPage", typeof(Views.EmployeesPage));
            Routing.RegisterRoute("AdminSettingsPage", typeof(Views.AdminSettingsPage));
            Routing.RegisterRoute("AddEmployeePage", typeof(Views.AddEmployeePage));
            Routing.RegisterRoute("AddTaskPage", typeof(Views.AddTaskPage));
            Routing.RegisterRoute("AddProjectPage", typeof(Views.AddProjectPage));

            // Store reference to sidebar
            sidebarInstance = this.sidebar;
        }

        // Method to refresh the sidebar
        public void RefreshSidebar()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Manually refreshing sidebar");

                if (sidebarInstance != null)
                {
                    // Force the sidebar to recreate with the current role
                    sidebarInstance.BindingContext = SidebarViewModel.Create();
                    System.Diagnostics.Debug.WriteLine("Sidebar refreshed successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Sidebar reference is null");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing sidebar: {ex.Message}");
            }
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            // Check if navigating to leaves page
            // Fix: Convert Uri to string and then check if it contains LeaveRequestPage
            if (args.Target.Location.ToString().Contains("LeaveRequestPage"))
            {
                System.Diagnostics.Debug.WriteLine($"Navigating to LeaveRequestPage, refreshing sidebar");

                // Force sidebar refresh
                Preferences.Set("ForceRefreshSidebar", true);
                RefreshSidebar();
            }
        }
    }
}