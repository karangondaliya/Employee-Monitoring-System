using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Views
{
    public partial class AddProjectPage : ContentPage
    {
        public AddProjectPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Load team members when page appears
            if (BindingContext is AddProjectViewModel viewModel)
            {
                viewModel.LoadTeamMembersCommand.Execute(null);
            }
        }
    }
}