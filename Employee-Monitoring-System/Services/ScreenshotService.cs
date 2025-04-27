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
        private readonly TimeSpan _idleCheckInterval = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _idleThreshold = TimeSpan.FromSeconds(10);
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7227/api/Screenshots/UploadFile"; // API endpoint
        private readonly TimeSpan _captureInterval = TimeSpan.FromSeconds(30); // Changed to 30 seconds
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
            await Application.Current.MainPage.DisplayAlert("Tracking Started", "Screenshot capturing & Idle Detection has started.", "OK");

            _ = Task.Run(async () =>
            {
                while (_isRunning)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await CaptureAndUploadScreenshot();
                    });

                    await Task.Delay(_captureInterval);
                }
            });

            _ = Task.Run(async () =>
            {
                while (_isRunning)
                {
                    await CheckIdleTime();
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

                // Show error alert on main thread
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await Application.Current.MainPage.DisplayAlert("Screenshot Error",
                        $"Error capturing screenshot: {ex.Message}", "OK");
                });
            }
        }

        private async Task CheckIdleTime()
        {
            var idleTime = IdleTimeDetector.GetIdleTime();

            if (idleTime >= _idleThreshold)
            {
                if (!_idleAlertShown)
                {
                    _idleAlertShown = true;
                    Console.WriteLine("User has been idle for over 30 seconds.");

                    // Display alert on main thread when idle time threshold is reached
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Idle Detection",
                            "You have been idle for 10 seconds. Your activity is being monitored.", "OK");
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
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        await Application.Current.MainPage.DisplayAlert("Error",
                            "Authentication token is missing. Please log in again.", "OK");
                    });
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

                    // Optional: Show success message (uncomment if you want to show upload confirmation)
                    // await MainThread.InvokeOnMainThreadAsync(async () => {
                    //     await Application.Current.MainPage.DisplayAlert("Screenshot", 
                    //         "Screenshot captured and uploaded successfully.", "OK");
                    // });
                }
                else
                {
                    Console.WriteLine($"Failed to upload screenshot: {response.ReasonPhrase}");

                    // Show error on main thread
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        await Application.Current.MainPage.DisplayAlert("Upload Error",
                            $"Failed to upload screenshot: {response.ReasonPhrase}", "OK");
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading screenshot: {ex.Message}");

                // Show error on main thread
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await Application.Current.MainPage.DisplayAlert("Upload Error",
                        $"Error uploading screenshot: {ex.Message}", "OK");
                });
            }
        }
    }
}