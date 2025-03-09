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

namespace Employee_Monitoring_System.Views
{
    public partial class LeaveRequestPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        public ObservableCollection<LeaveRequest> LeaveRequests { get; set; } = new ObservableCollection<LeaveRequest>();

        public LeaveRequestPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
            LoadLeaveRequests();
            BindingContext = this;  // Binding to UI
        }
        private async void LoadLeaveRequests()
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "Authentication required. Please log in again.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                int Id = int.Parse(await SecureStorage.GetAsync("UserId"));
                var response = await _httpClient.GetAsync($"LeaveRequests/GetByUserId/{Id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var leaveRequests = JsonConvert.DeserializeObject<List<LeaveRequest>>(json);

                    LeaveRequests.Clear();
                    foreach (var leave in leaveRequests)
                    {
                        LeaveRequests.Add(leave);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Failed to fetch leave requests.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnAddLeaveRequestClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddLeaveRequestPage());
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var leaveRequest = button?.BindingContext as LeaveRequest;
            if (leaveRequest == null) return;
            Debug.WriteLine($"Fetching Leave Request ID: {leaveRequest.Id}");
            await Navigation.PushAsync(new EditLeaveRequestPage(leaveRequest.Id));
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var leaveRequest = button?.BindingContext as LeaveRequest;
            if (leaveRequest == null) return;

            bool confirm = await DisplayAlert("Confirm", "Are you sure you want to delete this leave request?", "Yes", "No");
            if (!confirm) return;

            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "Authentication required. Please log in again.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.DeleteAsync($"LeaveRequests/{leaveRequest.Id}");

                if (response.IsSuccessStatusCode)
                {
                    LeaveRequests.Remove(leaveRequest);
                    await DisplayAlert("Success", "Leave request deleted successfully.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to delete leave request.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
