using System.Net.Http.Headers;
using System.Text.Json;
using Employee_Monitoring_System.Models;

public class HolidayService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public HolidayService(IPreferences preferences)
    {
        _httpClient = new HttpClient();
        _baseUrl = "https://localhost:7227/api/Holidays";

        // Set the default headers for the HttpClient
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private async Task SetAuthTokenAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        System.Diagnostics.Debug.WriteLine($"Auth token present: {!string.IsNullOrEmpty(token)}");

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<Holiday>> GetAllHolidaysAsync()
    {
        try
        {
            await SetAuthTokenAsync();

            System.Diagnostics.Debug.WriteLine("Requesting holidays from API...");
            var response = await _httpClient.GetAsync(_baseUrl);
            var content = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Holiday API Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Holiday API Response: {content}");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var holidays = JsonSerializer.Deserialize<List<Holiday>>(content, options);

                if (holidays != null && holidays.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully parsed {holidays.Count} holidays");
                    return holidays;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("API returned success but no holidays were found");
                    return new List<Holiday>();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Holiday API error: {response.StatusCode}");
                return new List<Holiday>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Holiday API exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new List<Holiday>();
        }
    }

    public async Task<List<Holiday>> GetHolidaysInRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            await SetAuthTokenAsync();

            // Convert dates to UTC format
            string startDate = start.ToString("o");
            string endDate = end.ToString("o");

            var url = $"{_baseUrl}/range?start={startDate}&end={endDate}";
            System.Diagnostics.Debug.WriteLine($"Requesting holidays in range from API: {url}");

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Holiday Range API Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Holiday Range API Response: {content}");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var holidays = JsonSerializer.Deserialize<List<Holiday>>(content, options);

                if (holidays != null && holidays.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully parsed {holidays.Count} holidays in range");
                    return holidays;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("API returned success but no holidays were found in range");
                    return new List<Holiday>();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Holiday Range API error: {response.StatusCode}");
                return new List<Holiday>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Holiday Range API exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new List<Holiday>();
        }
    }
}