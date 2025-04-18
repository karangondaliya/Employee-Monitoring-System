using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Input;
using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MvvmHelpers;
using Newtonsoft.Json;

namespace Employee_Monitoring_System.ViewModels
{
    public class LeaveRequestViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<LeaveRequestItem> _leaveRequests;
        private ObservableCollection<Holiday> _upcomingHolidays;
        private string _currentTab = "MyRequests";

        // Leave Balance Properties
        private int _annualLeaveBalance = 12;
        private int _sickLeaveBalance = 3;
        private int _personalLeaveBalance = 1;
        private int _unpaidLeaveBalance = 0;

        public LeaveRequestViewModel()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
            LeaveRequests = new ObservableCollection<LeaveRequestItem>();
            UpcomingHolidays = new ObservableCollection<Holiday>();

            // Initialize commands
            CalendarViewCommand = new Command(ExecuteCalendarViewCommand);
            ViewFullReportCommand = new Command(ExecuteViewFullReportCommand);

            // Load data
            LoadData();
        }

        public async void LoadData()
        {
            await Task.WhenAll(
                LoadLeaveBalances(),
                LoadLeaveRequests(),
                LoadUpcomingHolidays()
            );
        }

        private async Task LoadLeaveBalances()
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Authentication required. Please log in again.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                int userId = int.Parse(await SecureStorage.GetAsync("UserId"));

                // Note: If your API doesn't have this endpoint, we'll just use the default values
                // var response = await _httpClient.GetAsync($"LeaveBalances/GetByUserId/{userId}");

                // Using default values for now
                AnnualLeaveBalance = 12;
                SickLeaveBalance = 3;
                PersonalLeaveBalance = 1;
                UnpaidLeaveBalance = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading leave balances: {ex.Message}");

                // Use default values if API fails
                AnnualLeaveBalance = 12;
                SickLeaveBalance = 3;
                PersonalLeaveBalance = 1;
                UnpaidLeaveBalance = 0;
            }
        }

        public async Task LoadLeaveRequests()
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Authentication required. Please log in again.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                int userId = int.Parse(await SecureStorage.GetAsync("UserId"));

                var response = await _httpClient.GetAsync($"LeaveRequests/GetByUserId/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var leaveRequests = JsonConvert.DeserializeObject<List<LeaveRequest>>(json);

                    LeaveRequests.Clear();

                    if (leaveRequests != null)
                    {
                        foreach (var leave in leaveRequests)
                        {
                            var item = new LeaveRequestItem
                            {
                                Id = leave.Id,
                                LeaveType = leave.LeaveType,
                                StartDate = leave.StartDate,
                                EndDate = leave.EndDate,
                                Status = leave.Status,
                                Description = leave.Description, // Changed from Reason
                                ApproverName = leave.ApproverName, // Changed from ApprovedBy
                                DateRange = FormatDateRange(leave.StartDate, leave.EndDate),
                                StatusColor = GetStatusColor(leave.Status),
                                CanCancel = leave.Status == "Pending",
                                ViewDetailsCommand = new Command(() => ViewLeaveRequestDetails(leave.Id)),
                                CancelCommand = new Command(() => CancelLeaveRequest(leave.Id))
                            };

                            LeaveRequests.Add(item);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error fetching leave requests. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading leave requests: {ex.Message}");

                // Add some sample data for testing if API fails
                LoadSampleLeaveRequests();
            }
        }

        public async Task LoadTeamRequests()
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Authentication required. Please log in again.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // If your API has this endpoint, uncomment this
                // var response = await _httpClient.GetAsync("LeaveRequests/GetTeamRequests");

                // For now, just show a message that this feature is coming soon
                await Application.Current.MainPage.DisplayAlert("Coming Soon", "Team requests feature will be available in a future update.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading team requests: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred loading team requests: {ex.Message}", "OK");
            }
        }

        private async Task LoadUpcomingHolidays()
        {
            try
            {
                var response = await _httpClient.GetAsync("Holidays/GetUpcoming");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var holidays = JsonConvert.DeserializeObject<List<Holiday>>(json);

                    UpcomingHolidays.Clear();

                    if (holidays != null)
                    {
                        foreach (var holiday in holidays)
                        {
                            UpcomingHolidays.Add(holiday);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading holidays: {ex.Message}");

                // Add some sample holidays for testing
                LoadSampleHolidays();
            }
        }

        private void LoadSampleLeaveRequests()
        {
            LeaveRequests.Clear();

            LeaveRequests.Add(new LeaveRequestItem
            {
                Id = 1,
                LeaveType = "Annual Leave",
                StartDate = new DateTime(2023, 12, 20),
                EndDate = new DateTime(2023, 12, 22),
                Status = "Approved",
                Description = "Year-end vacation",
                ApproverName = "John Smith",
                DateRange = "Dec 20 - Dec 22, 2023 • 3 days",
                StatusColor = Color.Parse("#22C55E"),
                CanCancel = false,
                ViewDetailsCommand = new Command(() => ViewLeaveRequestDetails(1))
            });

            LeaveRequests.Add(new LeaveRequestItem
            {
                Id = 2,
                LeaveType = "Sick Leave",
                StartDate = new DateTime(2023, 12, 5),
                EndDate = new DateTime(2023, 12, 5),
                Status = "Approved",
                Description = "Doctor's appointment",
                ApproverName = "John Smith",
                DateRange = "Dec 5, 2023 • 1 day",
                StatusColor = Color.Parse("#22C55E"),
                CanCancel = false,
                ViewDetailsCommand = new Command(() => ViewLeaveRequestDetails(2))
            });

            LeaveRequests.Add(new LeaveRequestItem
            {
                Id = 3,
                LeaveType = "Personal Leave",
                StartDate = new DateTime(2024, 1, 15),
                EndDate = new DateTime(2024, 1, 15),
                Status = "Pending",
                Description = "Family matter",
                ApproverName = null,
                DateRange = "Jan 15, 2024 • 1 day",
                StatusColor = Color.Parse("#F59E0B"),
                CanCancel = true,
                ViewDetailsCommand = new Command(() => ViewLeaveRequestDetails(3)),
                CancelCommand = new Command(() => CancelLeaveRequest(3))
            });
        }

        private void LoadSampleHolidays()
        {
            UpcomingHolidays.Clear();

            UpcomingHolidays.Add(new Holiday
            {
                HolidayId = 1,
                Title = "Christmas Day",
                Date = new DateTime(2023, 12, 25),
                Description = "Christmas Holiday"
            });

            UpcomingHolidays.Add(new Holiday
            {
                HolidayId = 2,
                Title = "Boxing Day",
                Date = new DateTime(2023, 12, 26),
                Description = "Boxing Day Holiday"
            });

            UpcomingHolidays.Add(new Holiday
            {
                HolidayId = 3,
                Title = "New Year's Day",
                Date = new DateTime(2024, 1, 1),
                Description = "New Year Holiday"
            });

            UpcomingHolidays.Add(new Holiday
            {
                HolidayId = 4,
                Title = "Martin Luther King Jr. Day",
                Date = new DateTime(2024, 1, 15),
                Description = "MLK Day"
            });
        }

        private void ViewLeaveRequestDetails(int leaveId)
        {
            // Since LeaveDetailsPage doesn't exist, we'll navigate to EditLeaveRequestPage instead
            Application.Current.MainPage.Navigation.PushAsync(new EditLeaveRequestPage(leaveId));
        }

        private async void CancelLeaveRequest(int leaveId)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Cancel", "Are you sure you want to cancel this leave request?", "Yes", "No");

            if (!confirm) return;

            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Authentication required. Please log in again.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.DeleteAsync($"LeaveRequests/{leaveId}");

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Leave request cancelled successfully.", "OK");
                    // Reload the leave requests
                    await LoadLeaveRequests();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to cancel leave request.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void ExecuteCalendarViewCommand()
        {
            // Since LeaveCalendarPage doesn't exist, show a "coming soon" message
            Application.Current.MainPage.DisplayAlert("Coming Soon", "Calendar view will be available in a future update.", "OK");
        }

        private void ExecuteViewFullReportCommand()
        {
            // Since LeaveReportPage doesn't exist, show a "coming soon" message
            Application.Current.MainPage.DisplayAlert("Coming Soon", "Full report view will be available in a future update.", "OK");
        }

        private string FormatDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate.Date == endDate.Date)
            {
                return $"{startDate:MMM d, yyyy} • 1 day";
            }

            int days = (endDate.Date - startDate.Date).Days + 1;
            return $"{startDate:MMM d} - {endDate:MMM d, yyyy} • {days} days";
        }

        private Color GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "approved" => Color.Parse("#22C55E"),  // Green
                "pending" => Color.Parse("#F59E0B"),   // Orange
                "rejected" => Color.Parse("#EF4444"),  // Red
                "cancelled" => Color.Parse("#64748B"), // Slate
                _ => Color.Parse("#64748B")            // Default slate
            };
        }

        // Properties
        public ObservableCollection<LeaveRequestItem> LeaveRequests
        {
            get => _leaveRequests;
            set => SetProperty(ref _leaveRequests, value);
        }

        public ObservableCollection<Holiday> UpcomingHolidays
        {
            get => _upcomingHolidays;
            set => SetProperty(ref _upcomingHolidays, value);
        }

        public string CurrentTab
        {
            get => _currentTab;
            set => SetProperty(ref _currentTab, value);
        }

        // Leave Balance Properties
        public int AnnualLeaveBalance
        {
            get => _annualLeaveBalance;
            set => SetProperty(ref _annualLeaveBalance, value);
        }

        public int SickLeaveBalance
        {
            get => _sickLeaveBalance;
            set => SetProperty(ref _sickLeaveBalance, value);
        }

        public int PersonalLeaveBalance
        {
            get => _personalLeaveBalance;
            set => SetProperty(ref _personalLeaveBalance, value);
        }

        public int UnpaidLeaveBalance
        {
            get => _unpaidLeaveBalance;
            set => SetProperty(ref _unpaidLeaveBalance, value);
        }

        // Progress Calculations
        public double AnnualLeaveProgress => _annualLeaveBalance / 20.0;
        public int AnnualLeaveRemaining => _annualLeaveBalance;

        public double SickLeaveProgress => _sickLeaveBalance / 15.0;
        public int SickLeaveRemaining => _sickLeaveBalance;

        public double PersonalLeaveProgress => _personalLeaveBalance / 5.0;
        public int PersonalLeaveRemaining => _personalLeaveBalance;

        public double UnpaidLeaveProgress => _unpaidLeaveBalance / 30.0;
        public int UnpaidLeaveRemaining => 30; // Unpaid leave is always available

        // Commands
        public ICommand CalendarViewCommand { get; }
        public ICommand ViewFullReportCommand { get; }
    }

    public class LeaveRequestItem
    {
        public int Id { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; } // Changed from Reason
        public string ApproverName { get; set; } // Changed from ApprovedBy
        public string DateRange { get; set; }
        public Color StatusColor { get; set; }
        public bool CanCancel { get; set; }
        public ICommand ViewDetailsCommand { get; set; }
        public ICommand CancelCommand { get; set; }
    }
}