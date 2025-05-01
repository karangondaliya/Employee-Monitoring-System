using Employee_Monitoring_System.ViewModels;

namespace Employee_Monitoring_System.Views;

public partial class AddEmployeePage : ContentPage
{
    private AddEmployeeViewModel _viewModel;

    public AddEmployeePage()
    {
        InitializeComponent();
        _viewModel = BindingContext as AddEmployeeViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel?.InitializeAsync();
    }
}