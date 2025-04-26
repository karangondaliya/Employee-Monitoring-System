using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Employee_Monitoring_System.Views
{
    public partial class UpdatePasswordPage : ContentPage
    {
        public UpdatePasswordPage()
        {
            InitializeComponent();
        }

        private async void OnUpdatePasswordClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text?.Trim();
            string oldPassword = OldPasswordEntry.Text;
            string newPassword = NewPasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                await DisplayAlert("Input Required", "Please fill in all fields to continue.", "OK");
                return;
            }

            try
            {
                using HttpClient client = new HttpClient();
                var requestBody = new
                {
                    Email = email,
                    OldPassword = oldPassword,
                    NewPassword = newPassword
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("https://localhost:7227/api/Users/update-password", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Your password has been updated successfully!", "OK");
                    await Navigation.PopAsync(); // Go back to login page
                }
                else
                {
                    var responseMessage = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Update Failed", responseMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Connection Error", $"Unable to update password: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Return to previous page (login)
        }
    }
}