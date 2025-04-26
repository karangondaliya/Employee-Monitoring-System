using System;
using System.Collections.ObjectModel;
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
        private ObservableCollection<KeyValuePair<string, string>> _technicalSkills;
        private ObservableCollection<string> _certifications;
        private DateTime _lastUpdated;

        public UserProfilePage()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/") };
            _technicalSkills = new ObservableCollection<KeyValuePair<string, string>>();
            _certifications = new ObservableCollection<string>();
            _lastUpdated = DateTime.UtcNow;

            LoadUserProfile();
        }

        private async void LoadUserProfile()
        {
            try
            {
                LoadingIndicator.IsVisible = true;

                var token = await SecureStorage.GetAsync("auth_token");
                var userId = await SecureStorage.GetAsync("UserId");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                {
                    await DisplayAlert("Error", "User not logged in", "OK");
                    await Shell.Current.GoToAsync("//LoginPage");
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
                    RoleLabel.Text = userData["role"]?.ToString();
                    PhoneNumberEntry.Text = userData["phoneNumber"]?.ToString();
                    LastLoginLabel.Text = FormatDateTime(userData["lastLogin"]?.ToString());
                    JobTitleEntry.Text = userData["jobTitle"]?.ToString();
                    AddressEntry.Text = userData["address"]?.ToString();

                    // Set active status
                    bool isActive = userData["isActive"] != null && userData["isActive"].Value<bool>();
                    StatusLabel.Text = isActive ? "Active" : "Inactive";
                    StatusLabel.TextColor = isActive ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F44336");

                    // Load Technical Skills
                    _technicalSkills.Clear();
                    SkillsStackLayout.Children.Clear();
                    var skills = userData["technicalSkills"];
                    if (skills != null && skills.Type != JTokenType.Null)
                    {
                        JObject skillsObject = skills as JObject;
                        if (skillsObject != null && skillsObject.Count > 0)
                        {
                            foreach (JProperty prop in skillsObject.Properties())
                            {
                                _technicalSkills.Add(new KeyValuePair<string, string>(prop.Name, prop.Value.ToString()));

                                // Add skill to layout
                                var skillLabel = new Label
                                {
                                    Text = $"{prop.Name} - {prop.Value}",
                                    TextColor = Color.FromArgb("#333333"),
                                    FontSize = 14
                                };
                                SkillsStackLayout.Children.Add(skillLabel);
                            }
                        }
                    }
                    UpdateSkillsVisibility();

                    // Load Certifications
                    _certifications.Clear();
                    CertificationsStackLayout.Children.Clear();
                    var certifications = userData["certifications"];
                    if (certifications != null && certifications.Type != JTokenType.Null)
                    {
                        JArray certificationsArray = certifications as JArray;
                        if (certificationsArray != null && certificationsArray.Count > 0)
                        {
                            foreach (var cert in certificationsArray)
                            {
                                string certText = cert.ToString();
                                _certifications.Add(certText);

                                // Add certification to layout
                                var certLabel = new Label
                                {
                                    Text = certText,
                                    TextColor = Color.FromArgb("#333333"),
                                    FontSize = 14
                                };
                                CertificationsStackLayout.Children.Add(certLabel);
                            }
                        }
                    }
                    UpdateCertificationsVisibility();

                    // Load Profile Image
                    var profileImage = userData["profileImageBase64"]?.ToString();
                    if (!string.IsNullOrEmpty(profileImage))
                    {
                        LoadProfileImage(profileImage);
                    }

                    // Fetch branch details if BranchId is available
                    var branchId = userData["branchId"]?.ToString();
                    if (!string.IsNullOrEmpty(branchId) && branchId != "null")
                    {
                        await LoadBranchDetails(branchId);
                    }
                    else
                    {
                        BranchLabel.Text = "Not Assigned";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Failed to load profile: {response.StatusCode}\n{errorContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }

        private void UpdateSkillsVisibility()
        {
            bool hasSkills = _technicalSkills.Count > 0;
            SkillsStackLayout.IsVisible = hasSkills;
            NoSkillsLabel.IsVisible = !hasSkills;
        }

        private void UpdateCertificationsVisibility()
        {
            bool hasCertifications = _certifications.Count > 0;
            CertificationsStackLayout.IsVisible = hasCertifications;
            NoCertificationsLabel.IsVisible = !hasCertifications;
        }

        private string FormatDateTime(string dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr) || dateTimeStr == "null")
                return "Never";

            if (DateTime.TryParse(dateTimeStr, out DateTime dateTime))
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return dateTimeStr;
        }

        private void LoadProfileImage(string base64String)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String))
                    return;

                // Convert Base64 to ImageSource
                byte[] imageBytes = Convert.FromBase64String(base64String);
                ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
            catch (Exception ex)
            {
                // Log error but don't disrupt the user experience
                System.Diagnostics.Debug.WriteLine($"Error loading profile image: {ex.Message}");
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
                    BranchLabel.Text = branchData["branchName"]?.ToString();
                }
                else
                {
                    BranchLabel.Text = "Unknown Branch";
                }
            }
            catch (Exception ex)
            {
                BranchLabel.Text = "Error Fetching Branch";
                System.Diagnostics.Debug.WriteLine($"Error loading branch: {ex.Message}");
            }
        }

        private async void OnUpdateProfileClicked(object sender, EventArgs e)
        {
            try
            {
                LoadingIndicator.IsVisible = true;

                // Create a patch document with all fields
                var patchDoc = new object[]
                {
                    new { op = "replace", path = "/fullName", value = FullNameEntry.Text },
                    new { op = "replace", path = "/email", value = EmailEntry.Text },
                    new { op = "replace", path = "/phoneNumber", value = PhoneNumberEntry.Text },
                    new { op = "replace", path = "/jobTitle", value = JobTitleEntry.Text },
                    new { op = "replace", path = "/address", value = AddressEntry.Text }
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
                    // Reload the profile to show the updates
                    LoadUserProfile();
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
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnChangePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                // Request permission to access the photo library
                var status = await Permissions.RequestAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Required", "Photo library access is needed to change your profile photo.", "OK");
                    return;
                }

                // Pick photo
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select Profile Image"
                });

                if (result == null)
                    return;

                var stream = await result.OpenReadAsync();

                // Convert to base64
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);

                    // Update UI immediately
                    ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));

                    // Send to server
                    await UploadProfileImage(base64String);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to change photo: {ex.Message}", "OK");
            }
        }

        private async Task UploadProfileImage(string base64Image)
        {
            try
            {
                LoadingIndicator.IsVisible = true;

                var token = await SecureStorage.GetAsync("auth_token");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var content = new
                {
                    profileImageBase64 = base64Image
                };

                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(content),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"Users/{_userId}/profile-image", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Failed to upload profile image: {response.StatusCode}\n{errorContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to upload profile image: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnAddSkillClicked(object sender, EventArgs e)
        {
            string skill = await DisplayPromptAsync("Add Skill", "Enter skill name:", "Add", "Cancel");
            if (string.IsNullOrEmpty(skill))
                return;

            string proficiency = await DisplayPromptAsync("Skill Proficiency", "Enter proficiency level (Beginner, Intermediate, Expert):", "Add", "Cancel", "e.g., Intermediate");
            if (string.IsNullOrEmpty(proficiency))
                return;

            _technicalSkills.Add(new KeyValuePair<string, string>(skill, proficiency));

            // Add to UI directly
            var skillLabel = new Label
            {
                Text = $"{skill} - {proficiency}",
                TextColor = Color.FromArgb("#333333"),
                FontSize = 14
            };
            SkillsStackLayout.Children.Add(skillLabel);
            UpdateSkillsVisibility();

            await UpdateSkillsAndCertifications();
        }

        private async void OnAddCertificationClicked(object sender, EventArgs e)
        {
            string certification = await DisplayPromptAsync("Add Certification", "Enter certification name:", "Add", "Cancel");
            if (string.IsNullOrEmpty(certification))
                return;

            _certifications.Add(certification);

            // Add to UI directly
            var certLabel = new Label
            {
                Text = certification,
                TextColor = Color.FromArgb("#333333"),
                FontSize = 14
            };
            CertificationsStackLayout.Children.Add(certLabel);
            UpdateCertificationsVisibility();

            await UpdateSkillsAndCertifications();
        }

        private async Task UpdateSkillsAndCertifications()
        {
            try
            {
                LoadingIndicator.IsVisible = true;

                // Convert ObservableCollection to Dictionary and List
                Dictionary<string, string> skills = new Dictionary<string, string>();
                foreach (var skill in _technicalSkills)
                {
                    skills[skill.Key] = skill.Value;
                }

                List<string> certifications = new List<string>(_certifications);

                // Create the JSON patch
                var patchDoc = new object[]
                {
                    new { op = "replace", path = "/technicalSkills", value = skills },
                    new { op = "replace", path = "/certifications", value = certifications }
                };

                var token = await SecureStorage.GetAsync("auth_token");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"Users/{_userId}")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(patchDoc),
                        Encoding.UTF8,
                        "application/json-patch+json")
                };

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _lastUpdated = DateTime.UtcNow;
                    await DisplayAlert("Success", "Skills and certifications updated successfully!", "OK");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Failed to update skills and certifications: {response.StatusCode}\n{responseContent}", "OK");

                    // Reload profile to restore server state
                    LoadUserProfile();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update skills and certifications: {ex.Message}", "OK");

                // Reload profile to restore server state
                LoadUserProfile();
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}