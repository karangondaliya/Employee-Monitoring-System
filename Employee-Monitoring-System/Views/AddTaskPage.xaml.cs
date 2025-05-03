using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Views
{
    public partial class AddTaskPage : ContentPage
    {
        public AddTaskPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Load team members when page appears
            if (BindingContext is AddTaskPageViewModel viewModel)
            {
                viewModel.LoadTeamMembersCommand.Execute(null);
            }
        }
    }
}