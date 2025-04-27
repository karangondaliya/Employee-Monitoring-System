using Employee_Monitoring_System.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Employee_Monitoring_System.Services
{
    public class EmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7227/api/";
        private readonly JsonSerializerOptions _jsonOptions;

        public EmployeeService()
        {
            // Create HttpClient with proper handler that ignores SSL errors in development
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Configure JSON options to be case insensitive
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
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

        public async Task<List<Employee>> GetEmployeesAsync(string searchQuery = null)
        {
            try
            {
                await SetAuthorizationHeader();

                var url = "Users";

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    url += $"?search={Uri.EscapeDataString(searchQuery)}";
                }

                System.Diagnostics.Debug.WriteLine($"Fetching employees from: {url}");

                int maxRetries = 3;
                int retryCount = 0;
                HttpResponseMessage response = null;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        response = await _httpClient.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        break;
                    }
                    catch (HttpRequestException ex) when (retryCount < maxRetries - 1)
                    {
                        retryCount++;
                        System.Diagnostics.Debug.WriteLine($"Connection failed. Retry {retryCount}/{maxRetries}: {ex.Message}");
                        await Task.Delay(1000 * retryCount);
                    }
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Failed to connect to the server after multiple attempts");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Raw API response: {jsonResponse}");

                // Handle different response formats
                List<Employee> employees;
                try
                {
                    // Try to parse as array first
                    employees = JsonSerializer.Deserialize<List<Employee>>(jsonResponse, _jsonOptions);
                }
                catch
                {
                    try
                    {
                        // If array fails, try to parse as single object and wrap in list
                        var employee = JsonSerializer.Deserialize<Employee>(jsonResponse, _jsonOptions);
                        employees = new List<Employee> { employee };
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"JSON parse error: {ex.Message}");
                        throw new JsonException("Could not parse API response", ex);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Received {employees?.Count ?? 0} employees from API");

                // Set defaults for any null values
                if (employees != null)
                {
                    foreach (var emp in employees)
                    {
                        if (string.IsNullOrEmpty(emp.FullName))
                        {
                            emp.FullName = "User " + emp.Id;
                        }

                        // Debug
                        System.Diagnostics.Debug.WriteLine($"Employee: {emp.FullName}, Email: {emp.Email}, Phone: {emp.Phone}");
                    }
                }

                return employees ?? new List<Employee>();
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON parsing error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Data Error",
                    "Error parsing employee data from the server.", "OK");
                return new List<Employee>();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching employees: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Connection Error",
                    "Unable to load employees. Please check your connection and try again.", "OK");
                return new List<Employee>();
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Request timed out");
                await Application.Current.MainPage.DisplayAlert("Connection Timeout",
                    "The request to the server timed out. Please try again later.", "OK");
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
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.GetAsync($"Users/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Employee>(_jsonOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting employee {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("Users", employee);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Employee>(_jsonOptions);
        }

        public async Task<Employee> UpdateEmployeeAsync(int id, Employee employee)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"Users/{id}", employee);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Employee>(_jsonOptions);
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"Users/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}