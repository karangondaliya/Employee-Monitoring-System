using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Employee_Monitoring_System.Models;

namespace Employee_Monitoring_System.Views
{
    public partial class AddLeaveRequestPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public AddLeaveRequestPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
        }

        private async void OnSubmitLeaveRequestClicked(object sender, EventArgs e)
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "Authentication token is missing. Please log in again.", "OK");
                    return;
                }

                var leaveRequest = new LeaveRequest
                {
                    LeaveType = LeaveTypeEntry.Text,
                    StartDate = StartDatePicker.Date,
                    EndDate = EndDatePicker.Date,
                    Status = "Pending",
                    UserId = int.Parse(await SecureStorage.GetAsync("UserId"))
                };

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("LeaveRequests", leaveRequest);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Leave request submitted.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Failed to submit leave request.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
