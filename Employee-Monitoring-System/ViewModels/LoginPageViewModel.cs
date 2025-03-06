using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.Views;

namespace Employee_Monitoring_System
{
    public class LoginPageViewModel : BindableObject
    {
        private readonly HttpClient _httpClient;

        public LoginPageViewModel()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7227/api/Users/login") };
            LoginCommand = new Command(async () => await LoginAsync());
        }

        private string _email;
        public string email
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
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter both Email and Password.", "OK");
                return;
            }

            var loginRequest = new LoginRequest { email = email, Password = Password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Users/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    string token = await response.Content.ReadAsStringAsync();

                    // Save token for future API requests (e.g., Secure Storage)
                    await SecureStorage.SetAsync("auth_token", token);

                    // Navigate to Dashboard
                    await Application.Current.MainPage.Navigation.PushAsync(new DashboardPage());
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
