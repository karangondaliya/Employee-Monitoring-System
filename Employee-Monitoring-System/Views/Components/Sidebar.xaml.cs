using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Views.Components
{
    public partial class Sidebar : ContentView
    {
        public Sidebar()
        {
            InitializeComponent();
            SidebarViewModel.Instance.LoadSidebarItems();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("UserRole"); // Use Preferences instead of SecureStorage
            SecureStorage.Remove("auth_token");

            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

}