using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Maui.Controls;

namespace Employee_Monitoring_System.Services
{
    public class ScreenshotService
    {
        private readonly TimeSpan _idleCheckInterval = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _idleThreshold = TimeSpan.FromMinutes(1);
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7227/api/Screenshots/UploadFile"; // API endpoint
        private readonly TimeSpan _captureInterval = TimeSpan.FromMinutes(1); // Capture every 5 minutes
        private bool _isRunning = false;
        private bool _idleAlertShown = false;
        public ScreenshotService()
        {
            _httpClient = new HttpClient();
        }

        public async Task StartCapturingAsync()
        {
            if (_isRunning) return;
            _isRunning = true;

            // Show alert message only once when the process starts
            await Application.Current.MainPage.DisplayAlert("Screenshot Capturing", "Screenshot capturing has started.", "OK");

            _ = Task.Run(async () =>
            {
                while (_isRunning)
                {
                    await CaptureAndUploadScreenshot();
                    await Task.Delay(_captureInterval);
                }
            });

            _ = Task.Run(async () =>
            {
                while (_isRunning)
                {
                    CheckIdleTime();
                    await Task.Delay(_idleCheckInterval);
                }
            });
        }

        public void StopCapturing()
        {
            _isRunning = false;
        }

        private async Task CaptureAndUploadScreenshot()
        {
            try
            {
                var screenshot = await Screenshot.CaptureAsync();
                var stream = await screenshot.OpenReadAsync(ScreenshotFormat.Png);

                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    await UploadScreenshot(memoryStream.ToArray()); // Send raw image bytes
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing screenshot: {ex.Message}");
            }
        }

        private void CheckIdleTime()
        {
            var idleTime = IdleTimeDetector.GetIdleTime();

            if (idleTime >= _idleThreshold)
            {
                if (!_idleAlertShown)
                {
                    _idleAlertShown = true;
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Idle Alert", "You have been idle for over 10 minutes.", "OK");
                    });
                }
            }
            else
            {
                _idleAlertShown = false; // Reset if user becomes active again
            }
        }

        private async Task UploadScreenshot(byte[] imageBytes)
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Authentication token is missing. Please log in again.", "OK");
                    return;
                }

                using var content = new MultipartFormDataContent();
                var streamContent = new ByteArrayContent(imageBytes);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                content.Add(streamContent, "imageFile", $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Screenshot uploaded successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to upload screenshot: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading screenshot: {ex.Message}");
            }
        }
    }
}