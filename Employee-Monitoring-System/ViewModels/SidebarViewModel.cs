using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
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

        public ICommand NavigateCommand { get; }

        public SidebarViewModel()
        {
            NavigateCommand = new Command<string>(Navigate);
        }

        private async void Navigate(string pageName)
        {
            ActivePage = pageName.ToLower(); // Ensure lowercase to match routes
            await Shell.Current.GoToAsync($"//{ActivePage}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
