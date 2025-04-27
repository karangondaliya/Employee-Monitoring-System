using Employee_Monitoring_System.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Employee_Monitoring_System.Services
{
    public class HolidayService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly IPreferences _preferences; // For retrieving stored token

        public HolidayService(IPreferences preferences)
        {
            _httpClient = new HttpClient();
            _baseUrl = "https://localhost:7227/api/Holidays"; // Replace with your actual API URL
            _preferences = preferences;

            // Set the default headers for the HttpClient
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void SetAuthToken()
        {
            var token = _preferences.Get("auth_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Holiday>> GetAllHolidaysAsync()
        {
            SetAuthToken();

            var response = await _httpClient.GetAsync(_baseUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var holidays = JsonSerializer.Deserialize<List<Holiday>>(content, options);
                return holidays;
            }

            return new List<Holiday>();
        }

        public async Task<List<Holiday>> GetHolidaysInRangeAsync(DateTime start, DateTime end)
        {
            SetAuthToken();

            // Convert dates to UTC format
            string startDate = start.ToString("o");
            string endDate = end.ToString("o");

            var url = $"{_baseUrl}/range?start={startDate}&end={endDate}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var holidays = JsonSerializer.Deserialize<List<Holiday>>(content, options);
                return holidays;
            }

            return new List<Holiday>();
        }
    }
}