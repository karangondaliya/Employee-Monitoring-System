using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Employee_Monitoring_System.Models;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Net.Http;
using MvvmHelpers;

namespace Employee_Monitoring_System.ViewModels
{
    public class LeaveRequestViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        public ObservableCollection<LeaveRequest> LeaveRequests { get; set; }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public LeaveRequestViewModel()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
            LeaveRequests = new ObservableCollection<LeaveRequest>();
            LoadLeaveRequests();

            EditCommand = new Command<LeaveRequest>(OnEditLeaveRequest);
            DeleteCommand = new Command<LeaveRequest>(OnDeleteLeaveRequest);
        }

        private async void LoadLeaveRequests()
        {
            var response = await _httpClient.GetAsync("LeaveRequests");
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
        }

        private async void OnEditLeaveRequest(LeaveRequest leave)
        {
            await Application.Current.MainPage.DisplayAlert("Edit", $"Editing leave request: {leave.LeaveType}", "OK");
        }

        private async void OnDeleteLeaveRequest(LeaveRequest leave)
        {
            LeaveRequests.Remove(leave);
            await Application.Current.MainPage.DisplayAlert("Delete", "Leave request deleted.", "OK");
        }
    }
}
