using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using Employee_Monitoring_System.Models;
using System.Diagnostics;
using System.Windows.Input;
using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Views
{
    public partial class LeaveRequestPage : ContentPage
    {
        private readonly LeaveRequestViewModel _viewModel;
        private bool isFirstLoad;

        public LeaveRequestPage()
        {
            InitializeComponent();

            // Initialize ViewModel and set as BindingContext
            _viewModel = new LeaveRequestViewModel();
            BindingContext = _viewModel;

            // Set the initial tab as selected
            UpdateTabSelection("MyRequests");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Get current role
                string currentRole = Preferences.Get("UserRole", "Employee");
                System.Diagnostics.Debug.WriteLine($"Leave page appearing with role: {currentRole}");

                // Set flag for sidebar refresh
                Preferences.Set("ForceRefreshSidebar", true);

                // Try to refresh the sidebar through AppShell
                if (Application.Current.MainPage is AppShell appShell)
                {
                    MainThread.BeginInvokeOnMainThread(() => {
                        appShell.RefreshSidebar();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing sidebar: {ex.Message}");
            }

            // Load leave data
            isFirstLoad = true;
            await _viewModel.LoadLeaveRequests();
        }

        private void OnAddLeaveRequestClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AddLeaveRequestPage());
        }

        private async void OnMyRequestsTabClicked(object sender, EventArgs e)
        {
            UpdateTabSelection("MyRequests");
            _viewModel.CurrentTab = "MyRequests";
            await _viewModel.LoadLeaveRequests();
        }

        private async void OnTeamRequestsTabClicked(object sender, EventArgs e)
        {
            UpdateTabSelection("TeamRequests");
            _viewModel.CurrentTab = "TeamRequests";
            await _viewModel.LoadTeamRequests();
        }

        private void OnLeavePolicyTabClicked(object sender, EventArgs e)
        {
            UpdateTabSelection("LeavePolicy");
            _viewModel.CurrentTab = "LeavePolicy";
            // Show coming soon message since we don't have this page yet
            Application.Current.MainPage.DisplayAlert("Coming Soon", "Leave policy view will be available in a future update.", "OK");
        }

        private void UpdateTabSelection(string selectedTab)
        {
            // Reset all tabs
            MyRequestsTab.BackgroundColor = Colors.Transparent;
            MyRequestsTab.TextColor = Color.Parse("#303846");

            TeamRequestsTab.BackgroundColor = Colors.Transparent;
            TeamRequestsTab.TextColor = Color.Parse("#303846");

            LeavePolicyTab.BackgroundColor = Colors.Transparent;
            LeavePolicyTab.TextColor = Color.Parse("#303846");

            // Set the selected tab
            switch (selectedTab)
            {
                case "MyRequests":
                    MyRequestsTab.BackgroundColor = Color.Parse("#303846");
                    MyRequestsTab.TextColor = Colors.White;
                    break;
                case "TeamRequests":
                    TeamRequestsTab.BackgroundColor = Color.Parse("#303846");
                    TeamRequestsTab.TextColor = Colors.White;
                    break;
                case "LeavePolicy":
                    LeavePolicyTab.BackgroundColor = Color.Parse("#303846");
                    LeavePolicyTab.TextColor = Colors.White;
                    break;
            }
        }

    }
}