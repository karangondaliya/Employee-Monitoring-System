//using Microcharts;
//using SkiaSharp;
using Microsoft.Maui;

namespace Employee_Monitoring_System
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
            BindingContext = this;

            //// Example chart data
            //ProductivityChart = new DonutChart
            //{
            //    Entries = new[]
            //    {
            //    new ChartEntry(40) { Label = "Productive", ValueLabel = "40%", Color = SKColor.Parse("#2ECC71") },
            //    new ChartEntry(30) { Label = "Meetings", ValueLabel = "30%", Color = SKColor.Parse("#3498DB") },
            //    new ChartEntry(30) { Label = "Idle", ValueLabel = "30%", Color = SKColor.Parse("#E74C3C") }
            //}
            //};
        }
        //public Chart ProductivityChart { get; }
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Navigate back to the Login Page
            await Navigation.PushAsync(new LoginPage());
        }
    }
}