using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Employee_Monitoring_System.Views
{
    public partial class UserProfilePage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private int _userId;

        public UserProfilePage()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
            LoadUserProfile();
        }

        private async void LoadUserProfile()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var userId = await SecureStorage.GetAsync("UserId");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                {
                    await DisplayAlert("Error", "User not logged in", "OK");
                    return;
                }

                _userId = int.Parse(userId);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Fetch user details
                var response = await _httpClient.GetAsync($"Users/{_userId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var userData = JObject.Parse(jsonResponse); // Deserialize JSON dynamically

                    // Fill UI fields with fetched data
                    FullNameEntry.Text = userData["fullName"]?.ToString();
                    EmailEntry.Text = userData["email"]?.ToString();
                    RoleEntry.Text = userData["role"]?.ToString();
                    PhoneNumberEntry.Text = userData["phoneNumber"]?.ToString();
                    LastLoginLabel.Text = userData["lastLogin"]?.ToString();

                    // Fetch branch details if BranchId is available
                    var branchId = userData["branchId"]?.ToString();
                    if (!string.IsNullOrEmpty(branchId))
                    {
                        await LoadBranchDetails(branchId);
                    }
                    else
                    {
                        BranchEntry.Text = "Not Assigned";
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Failed to load profile", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task LoadBranchDetails(string branchId)
        {
            try
            {
                var branchResponse = await _httpClient.GetAsync($"Branches/{branchId}");

                if (branchResponse.IsSuccessStatusCode)
                {
                    var branchJson = await branchResponse.Content.ReadAsStringAsync();
                    var branchData = JObject.Parse(branchJson);

                    // Set Branch Name
                    BranchEntry.Text = branchData["branchName"]?.ToString();
                }
                else
                {
                    BranchEntry.Text = "Unknown Branch";
                }
            }
            catch (Exception ex)
            {
                BranchEntry.Text = "Error Fetching Branch";
            }
        }

        private async void OnUpdateProfileClicked(object sender, EventArgs e)
        {
            try
            {
                var patchDoc = new[]
                {
                    new { op = "replace", path = "/fullName", value = FullNameEntry.Text },
                    new { op = "replace", path = "/email", value = EmailEntry.Text },
                    new { op = "replace", path = "/phoneNumber", value = PhoneNumberEntry.Text }
                };

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "User not logged in", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"Users/{_userId}")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json-patch+json")
                };

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync(); // Get detailed response

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Profile updated successfully!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"Failed to update profile\n{response.StatusCode}\n{responseContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
