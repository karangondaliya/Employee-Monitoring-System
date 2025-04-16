using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Employee_Monitoring_System.Models;

namespace Employee_Monitoring_System.ViewModels
{
    public class ProjectsViewModel : BaseViewModel
    {
        public ObservableCollection<Project> Projects { get; set; }
        public ObservableCollection<string> Statuses { get; set; }
        public bool IsAdmin { get; set; }
        public string SearchQuery { get; set; }
        public string SelectedStatus { get; set; }

        public ICommand NewProjectCommand { get; set; }
        public ICommand ViewDetailsCommand { get; set; }

        public ProjectsViewModel()
        {
            // Set user role (replace with actual role-fetching logic)
            var userRole = Preferences.Get("UserRole", "Employee");
            IsAdmin = userRole == "Admin";

            // Sample projects
            Projects = new ObservableCollection<Project>
            {
                new Project
                {
                    Title = "Website Redesign",
                    Description = "Complete overhaul of company website with new branding",
                    Progress = 65,
                    Deadline = new DateTime(2023, 12, 15),
                    Team = "Jane Cooper, Robert Fox +1 more",
                    Status = "In Progress"
                },
                new Project
                {
                    Title = "Mobile App Development",
                    Description = "Creating iOS and Android versions of our flagship product",
                    Progress = 25,
                    Deadline = new DateTime(2024, 2, 28),
                    Team = "Cody Fisher, Esther Howard +1 more",
                    Status = "Planning"
                },
                new Project
                {
                    Title = "Marketing Campaign",
                    Description = "Q4 digital marketing campaign for product launch",
                    Progress = 100,
                    Deadline = new DateTime(2023, 10, 1),
                    Team = "Cameron Williamson, Courtney Henry",
                    Status = "Completed"
                },
                new Project
                {
                    Title = "Database Migration",
                    Description = "Transition from legacy database to new cloud platform",
                    Progress = 40,
                    Deadline = new DateTime(2024, 1, 15),
                    Team = "Wade Warren, Dianne Russell +2 more",
                    Status = "In Progress"
                }
            };

            // Sample statuses
            Statuses = new ObservableCollection<string>
            {
                "All Statuses",
                "Planning",
                "In Progress",
                "Completed"
            };

            // Commands
            NewProjectCommand = new Command(OnNewProject);
            ViewDetailsCommand = new Command<Project>(OnViewDetails);
        }

        private async void OnNewProject()
        {
            // Logic to create a new project
            await Application.Current.MainPage.DisplayAlert("New Project", "New Project button clicked!", "OK");
        }

        private async void OnViewDetails(Project project)
        {
            if (project != null)
            {
                // Navigate to project details page
                await Application.Current.MainPage.DisplayAlert("Project Details", $"Viewing details for {project.Title}", "OK");
            }
        }
    }
}