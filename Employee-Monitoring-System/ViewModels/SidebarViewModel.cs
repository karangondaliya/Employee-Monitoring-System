using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace Employee_Monitoring_System.ViewModels
{
    public class SidebarViewModel : INotifyPropertyChanged
    {
        private static SidebarViewModel _instance;
        public static SidebarViewModel Instance => _instance ??= new SidebarViewModel();

        private string _activePage;
        public string ActivePage
        {
            get => _activePage;
            set
            {
                _activePage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> SidebarItems { get; set; }
        public ICommand NavigateCommand { get; }

        public SidebarViewModel()
        {
            SidebarItems = new ObservableCollection<string>();
            NavigateCommand = new Command<string>(Navigate);
            LoadSidebarItems(); // Ensure sidebar loads on startup
        }

        public void LoadSidebarItems()
        {
            SidebarItems.Clear();
            SidebarItems.Add("Dashboard");

            string userRole = Preferences.Get("UserRole", "Employee"); // Use Preferences instead of SecureStorage

            if (userRole == "Admin")
            {
                SidebarItems.Add("Manage Employees");
                SidebarItems.Add("Manage Projects");
                SidebarItems.Add("Notifications");
                SidebarItems.Add("Manage Branches");
                SidebarItems.Add("App Settings");
                SidebarItems.Add("View Screenshots");
            }
            else if (userRole == "TeamLead")
            {
                SidebarItems.Add("Manage Tasks");
                SidebarItems.Add("Approve Leave Requests");
            }
            else if (userRole == "Employee")
            {
                SidebarItems.Add("My Tasks");
                SidebarItems.Add("My Attendance");
                SidebarItems.Add("My Leaves");
            }

            OnPropertyChanged(nameof(SidebarItems)); // Notify UI of updates
        }

        private async void Navigate(string pageName)
        {
            ActivePage = pageName.ToLower();
            await Shell.Current.GoToAsync($"//{ActivePage}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
