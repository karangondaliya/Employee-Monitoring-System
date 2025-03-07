using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.Views;
using Microsoft.Maui.Storage;

namespace Employee_Monitoring_System
{
    public class LoginPageViewModel : BindableObject
    {
        private readonly HttpClient _httpClient;

        public LoginPageViewModel()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/Users/") };
            LoginCommand = new Command(async () => await LoginAsync());
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter both Email and Password.", "OK");
                return;
            }

            var loginRequest = new LoginRequest { email = Email, Password = Password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (loginResponse != null)
                    {
                        // Save token and user role securely
                        await SecureStorage.SetAsync("auth_token", loginResponse.Token);
                        Preferences.Set("UserRole", loginResponse.Role);

                        // Navigate to DashboardPage
                        await Shell.Current.GoToAsync(nameof(DashboardPage));
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Invalid response from server.", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Login Failed", "Invalid credentials", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }

}
