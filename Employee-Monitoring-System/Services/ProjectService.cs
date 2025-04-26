using Employee_Monitoring_System.Models;
using System.Net.Http.Json;

namespace Employee_Monitoring_System.Services
{
    public class ProjectService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7227/api/"; // Same as your login service

        public ProjectService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }

        private async Task SetAuthorizationHeader()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Project>> GetProjectsAsync(string searchQuery = null, string status = null)
        {
            try
            {
                await SetAuthorizationHeader();

                var url = "Projects"; // Your projects endpoint
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(searchQuery))
                    queryParams.Add($"search={Uri.EscapeDataString(searchQuery)}");
                if (!string.IsNullOrEmpty(status) && status != "All Statuses")
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");

                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                System.Diagnostics.Debug.WriteLine($"Fetching projects from: {url}");

                // Use HttpClient.GetAsync to get the raw response
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Read the raw JSON response for debugging
                var jsonResponse = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Raw API response: {jsonResponse}");

                // Deserialize the JSON into your Project objects
                var projects = await response.Content.ReadFromJsonAsync<List<Project>>();
                System.Diagnostics.Debug.WriteLine($"Received {projects?.Count ?? 0} projects from API");

                return projects ?? new List<Project>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching projects: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Connection Error",
                    "Unable to load projects. Please check your connection and try again.", "OK");
                return new List<Project>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "An unexpected error occurred while loading projects.", "OK");
                return new List<Project>();
            }
        }

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            await SetAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<Project>($"Projects/{id}");
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("Projects", project);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Project>();
        }

        public async Task<Project> UpdateProjectAsync(int id, Project project)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"Projects/{id}", project);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Project>();
        }

        public async Task DeleteProjectAsync(int id)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"Projects/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}