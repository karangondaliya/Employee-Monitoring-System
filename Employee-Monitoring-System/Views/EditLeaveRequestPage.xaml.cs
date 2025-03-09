using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using Employee_Monitoring_System.Models;
using System.Diagnostics;

namespace Employee_Monitoring_System.Views
{
    public partial class EditLeaveRequestPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly int _leaveRequestId;

        public List<string> LeaveTypes { get; } = new List<string> { "Sick Leave", "Casual Leave", "Paid Leave", "Unpaid Leave" };
        public List<string> StatusOptions { get; } = new List<string> { "Pending", "Approved", "Rejected" };

        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int? ApproverId { get; set; }

        public EditLeaveRequestPage(int leaveRequestId)
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
            _leaveRequestId = leaveRequestId;
            Debug.WriteLine($"Fetching Leave Request ID: {_leaveRequestId}");
            LoadLeaveRequest();
            BindingContext = this;
        }

        private async void LoadLeaveRequest()
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
                var response = await _httpClient.GetAsync($"LeaveRequests/{_leaveRequestId}");

                Debug.WriteLine($"Response Status Code: {response.StatusCode}");
                Debug.WriteLine($"Response Content: {await response.Content.ReadAsStringAsync()}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var leaveRequest = JsonConvert.DeserializeObject<LeaveRequest>(json);

                    LeaveType = leaveRequest.LeaveType;
                    StartDate = leaveRequest.StartDate;
                    EndDate = leaveRequest.EndDate;
                    Status = leaveRequest.Status;
                    ApproverId = leaveRequest.ApproverId;

                    OnPropertyChanged(nameof(LeaveType));
                    OnPropertyChanged(nameof(StartDate));
                    OnPropertyChanged(nameof(EndDate));
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(ApproverId));
                }
                else
                {
                    await DisplayAlert("Error", "Failed to load leave request details.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "Authentication required. Please log in again.", "OK");
                    return;
                }

                var patchData = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(LeaveType)) patchData["LeaveType"] = LeaveType;
                if (StartDate != default) patchData["StartDate"] = StartDate;
                if (EndDate != default) patchData["EndDate"] = EndDate;
                if (!string.IsNullOrEmpty(Status)) patchData["Status"] = Status;
                if (ApproverId.HasValue) patchData["ApproverId"] = ApproverId;

                var json = JsonConvert.SerializeObject(patchData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"LeaveRequests/{_leaveRequestId}")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Leave request updated successfully!", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Failed to update leave request.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }


        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
