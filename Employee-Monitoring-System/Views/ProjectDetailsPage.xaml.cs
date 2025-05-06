using Microsoft.Maui.Controls;
using Employee_Monitoring_System.ViewModels;
using Employee_Monitoring_System.Models;

namespace Employee_Monitoring_System.Views
{
    [QueryProperty(nameof(ProjectId), "id")]
    public partial class ProjectDetailsPage : ContentPage
    {
        private int _projectId;

        public int ProjectId
        {
            get => _projectId;
            set
            {
                _projectId = value;
                LoadProject(_projectId);
            }
        }

        public ProjectDetailsPage()
        {
            InitializeComponent();
        }

        private void LoadProject(int projectId)
        {
            if (BindingContext is ProjectDetailsViewModel viewModel)
            {
                viewModel.LoadProject(projectId);
            }
        }
    }
}