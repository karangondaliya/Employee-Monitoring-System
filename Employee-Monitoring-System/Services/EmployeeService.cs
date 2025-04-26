using Employee_Monitoring_System.Models;
using System.Net.Http.Json;

namespace Employee_Monitoring_System.Services
{
    public class EmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7227/api/"; // Match your existing API base URL

        public EmployeeService()
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

        public async Task<List<Employee>> GetEmployeesAsync(string searchQuery = null, string department = null)
        {
            try
            {
                await SetAuthorizationHeader();

                var url = "Employees"; // Adjust to your API endpoint
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(searchQuery))
                    queryParams.Add($"search={Uri.EscapeDataString(searchQuery)}");
                if (!string.IsNullOrEmpty(department) && department != "All Departments")
                    queryParams.Add($"department={Uri.EscapeDataString(department)}");

                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                System.Diagnostics.Debug.WriteLine($"Fetching employees from: {url}");

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Raw API response: {jsonResponse}");

                var employees = await response.Content.ReadFromJsonAsync<List<Employee>>();
                System.Diagnostics.Debug.WriteLine($"Received {employees?.Count ?? 0} employees from API");

                return employees ?? new List<Employee>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching employees: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Connection Error",
                    "Unable to load employees. Please check your connection and try again.", "OK");
                return new List<Employee>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "An unexpected error occurred while loading employees.", "OK");
                return new List<Employee>();
            }
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            await SetAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<Employee>($"Employees/{id}");
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("Employees", employee);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Employee>();
        }

        public async Task<Employee> UpdateEmployeeAsync(int id, Employee employee)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"Employees/{id}", employee);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Employee>();
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"Employees/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}