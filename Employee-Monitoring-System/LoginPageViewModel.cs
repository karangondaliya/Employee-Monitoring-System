using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Employee_Monitoring_System
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        public Command LoginCommand { get; }

        public LoginPageViewModel()
        {
            LoginCommand = new Command(OnLogin);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnLogin()
        {
            // Add authentication logic here
            // Example:
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                // Authentication logic
            }
        }
    }
}
