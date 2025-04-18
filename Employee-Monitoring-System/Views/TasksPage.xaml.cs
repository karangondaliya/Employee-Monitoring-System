using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Views
{
    public partial class TasksPage : ContentPage
    {
        public TasksPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Refresh the tasks when the page appears
            if (BindingContext is TasksPageViewModel viewModel)
            {
                viewModel.LoadTasksCommand.Execute(null);
            }
        }
    }
}