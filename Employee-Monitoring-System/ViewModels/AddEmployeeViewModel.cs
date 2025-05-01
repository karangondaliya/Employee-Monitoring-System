using Employee_Monitoring_System.Models;
using Employee_Monitoring_System.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Employee_Monitoring_System.ViewModels
{
    public class AddEmployeeViewModel : BaseViewModel
    {
        private readonly EmployeeService _employeeService;
        private bool _isLoading;
        private string _fullName;
        private string _email;
        private string _phoneNumber;
        private string _password;
        private string _confirmPassword;
        private string _address;
        private string _jobTitle;
        private bool _isActive = true;
        private string _selectedRole;
        private Models.Branch _selectedBranch;
        private bool _hasProfileImage;
        private ImageSource _profileImageSource;
        private string _profileImageBase64;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string JobTitle
        {
            get => _jobTitle;
            set => SetProperty(ref _jobTitle, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public Models.Branch SelectedBranch
        {
            get => _selectedBranch;
            set
            {
                if (SetProperty(ref _selectedBranch, value) && value != null)
                {
                    BranchId = value.Id;
                    BranchName = value.Name;
                }
            }
        }

        public int BranchId { get; private set; }
        public string BranchName { get; private set; }

        public bool HasProfileImage
        {
            get => _hasProfileImage;
            set => SetProperty(ref _hasProfileImage, value);
        }

        public ImageSource ProfileImageSource
        {
            get => _profileImageSource;
            set => SetProperty(ref _profileImageSource, value);
        }

        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string>();
        public ObservableCollection<Models.Branch> Branches { get; } = new ObservableCollection<Models.Branch>();

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand UploadPhotoCommand { get; private set; }

        public AddEmployeeViewModel()
        {
            _employeeService = new EmployeeService();

            SaveCommand = new Command(async () => await SaveEmployee());
            CancelCommand = new Command(async () => await CancelAndGoBack());
            UploadPhotoCommand = new Command(async () => await UploadPhoto());

            // Initialize with default values
            Roles.Add("Employee");
            Roles.Add("TeamLead");
            Roles.Add("Admin");
            SelectedRole = "Employee";
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                // Load branches
                var branches = await _employeeService.GetBranchesAsync();
                if (branches != null)
                {
                    Branches.Clear();
                    foreach (var branch in branches)
                    {
                        Branches.Add(branch);
                    }

                    // Select first branch by default if available
                    if (Branches.Count > 0)
                    {
                        SelectedBranch = Branches[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing add employee form: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Failed to load form data. Please try again.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveEmployee()
        {
            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error",
                    "Please fill in all required fields: Name, Email, and Password.", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error",
                    "Passwords do not match.", "OK");
                return;
            }

            try
            {
                IsLoading = true;

                // Create new employee object
                var newEmployee = new Employee
                {
                    FullName = FullName,
                    Email = Email,
                    Password = Password,
                    PhoneNumber = PhoneNumber,
                    Role = SelectedRole,
                    BranchId = BranchId,
                    BranchName = BranchName,
                    IsActive = IsActive,
                    JobTitle = JobTitle,
                    Address = Address,
                    ProfileImageBase64 = _profileImageBase64
                };

                // Call API to create employee
                var createdEmployee = await _employeeService.CreateEmployeeAsync(newEmployee);

                await Application.Current.MainPage.DisplayAlert("Success",
                    "Employee created successfully!", "OK");

                // Navigate back to employees list
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating employee: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Failed to create employee. Please try again.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelAndGoBack()
        {
            // Navigate back
            await Shell.Current.GoToAsync("..");
        }

        private async Task UploadPhoto()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Profile Image",
                    FileTypes = FilePickerFileType.Images
                });

                if (result == null)
                    return;

                var stream = await result.OpenReadAsync();

                // Create a memory stream to convert to Base64
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    // Convert to Base64
                    byte[] fileBytes = memoryStream.ToArray();
                    _profileImageBase64 = Convert.ToBase64String(fileBytes);

                    // Set image for display
                    memoryStream.Position = 0;
                    ProfileImageSource = ImageSource.FromStream(() => new MemoryStream(fileBytes));
                    HasProfileImage = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error selecting image: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Failed to load image. Please try another image.", "OK");
            }
        }
    }
}