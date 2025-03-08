using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Employee_Monitoring_System.Models;
using Microsoft.AspNetCore.JsonPatch;
using MvvmHelpers;

namespace Employee_Monitoring_System.ViewModels
{
    public class NotificationsViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        public Command<Notification> MarkAsReadCommand { get; }
        public ObservableCollection<Notification> Notifications { get; set; } = new ObservableCollection<Notification>();

        public NotificationsViewModel()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/Notifications/") };
            LoadNotifications();
        }

        private async Task LoadNotifications()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "User not logged in", "OK");
                    return;
                }

                var userId = await SecureStorage.GetAsync("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "User ID not found", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"GetUserNotifications/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("API Error", $"Failed to fetch notifications: {errorContent}", "OK");
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var notifications = JsonConvert.DeserializeObject<List<Notification>>(jsonResponse);

                Notifications.Clear();
                foreach (var notification in notifications)
                {
                    Notifications.Add(notification);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }


        public async void MarkAsRead(Notification notification)
        {
            var patchDoc = new JsonPatchDocument<Notification>();
            patchDoc.Replace(n => n.IsRead, true);

                var json = JsonConvert.SerializeObject(patchDoc);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PatchAsync($"https://localhost:7227/api/Notifications/{notification.Id}", content);
                    notification.IsRead = true;
                }
    }

}
