using Employee_Monitoring_System.Services;

namespace Employee_Monitoring_System
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
