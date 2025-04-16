using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using Employee_Monitoring_System.Models;

namespace Employee_Monitoring_System.ViewModels
{
    public class SidebarViewModel : INotifyPropertyChanged
    {
        private static SidebarViewModel _instance = new SidebarViewModel();
        public static SidebarViewModel Instance => _instance;

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

        public ObservableCollection<SidebarItem> SidebarItems { get; set; }
        public ICommand NavigateCommand { get; }

        public SidebarViewModel()
        {
            SidebarItems = new ObservableCollection<SidebarItem>();
            NavigateCommand = new Command<SidebarItem>(Navigate);
            LoadSidebarItems(); // Ensure sidebar loads on startup
        }

        public void LoadSidebarItems()
        {
            SidebarItems.Clear();
            SidebarItems.Add(new SidebarItem { Title = "Dashboard", Icon = "dashboard.png" });

            string userRole = Preferences.Get("UserRole", "Employee"); // Use Preferences instead of SecureStorage

            if (userRole == "Admin")
            {
                SidebarItems.Add(new SidebarItem { Title = "Manage Employees", Icon = "users.png" });
                SidebarItems.Add(new SidebarItem { Title = "Manage Projects", Icon = "briefcase.png" });
                SidebarItems.Add(new SidebarItem { Title = "Manage Notifications", Icon = "notification_icon.png" });
                SidebarItems.Add(new SidebarItem { Title = "Manage Branches", Icon = "office.png" });
                SidebarItems.Add(new SidebarItem { Title = "Settings", Icon = "settings.png" });
                SidebarItems.Add(new SidebarItem { Title = "View Screenshots", Icon = "landscape.png" });
                SidebarItems.Add(new SidebarItem { Title = "Track Activity", Icon = "task.png" });
                SidebarItems.Add(new SidebarItem { Title = "View Leaves", Icon = "calendar_white.png" });
            }
            else if (userRole == "TeamLead")
            {
                SidebarItems.Add(new SidebarItem { Title = "Manage Tasks", Icon = "to_do_list.png" });
                SidebarItems.Add(new SidebarItem { Title = "Manage Leaves", Icon = "calendar_white.png" });
                SidebarItems.Add(new SidebarItem { Title = "Projects", Icon = "briefcase.png" });
            }
            else if (userRole == "Employee")
            {
                SidebarItems.Add(new SidebarItem { Title = "My Tasks", Icon = "to_do_list.png" });
                SidebarItems.Add(new SidebarItem { Title = "My Leaves", Icon = "calendar_white.png" });
                SidebarItems.Add(new SidebarItem { Title = "My Projects", Icon = "briefcase.png" });
            }

            OnPropertyChanged(nameof(SidebarItems)); // Notify UI of updates
        }

        private async void Navigate(SidebarItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Title))
                return;

            ActivePage = item.Title.ToLower();

            if (item.Title is "My Leaves" or "View Leaves" or "Manage Leaves")
            {
                await Shell.Current.GoToAsync("//LeaveRequestPage");
            }
            if(item.Title is "My Projects" or "Projects" or "Manage Projects")
            {
                await Shell.Current.GoToAsync("//ProjectsPage");
            }
            else
            {
                await Shell.Current.GoToAsync($"//{ActivePage}");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
