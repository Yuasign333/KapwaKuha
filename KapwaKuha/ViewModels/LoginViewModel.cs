using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    // Shared by AdminLogin.xaml and DonorLogin.xaml
    // Role is passed from code-behind: "Admin" | "Donor"
    public class LoginViewModel : ObservableObject
    {
        public UserModel CurrentUser { get; }

        public ICommand LoginCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        private bool _errorVisible;
        public bool ErrorVisible
        {
            get => _errorVisible;
            set { _errorVisible = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public LoginViewModel(string role)
        {
            CurrentUser = new UserModel { Role = role };

            LoginCommand = new RelayCommand(ExecuteLogin);
            BackCommand = new RelayCommand(_ => NavigationService.Navigate(new View.ChooseRoleWindow()));
            ForgotPasswordCommand = new RelayCommand(_ =>
                MessageBox.Show("Contact your system admin to reset your password.",
                    "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information));
        }

        private async void ExecuteLogin(object? parameter)
        {
            if (parameter is PasswordBox pb)
                CurrentUser.Password = pb.Password;

            if (string.IsNullOrWhiteSpace(CurrentUser.UserID) ||
                string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                ErrorMessage = "Please enter your ID/username and password.";
                ErrorVisible = true;
                return;
            }

            if (CurrentUser.Role == "Admin")
            {
                var (ok, userId, fullName) =
                    await KapwaDataService.LoginAdmin(CurrentUser.UserID, CurrentUser.Password);

                if (ok)
                {
                    ErrorVisible = false;
                    UserSession.UserId = userId;
                    UserSession.Username = userId;
                    UserSession.FullName = fullName;
                    UserSession.Role = "Admin";
                    NavigationService.Navigate(new View.AdminDashboardWindow(userId));
                }
                else
                {
                    ErrorMessage = "Invalid Agent ID or password.";
                    ErrorVisible = true;
                }
            }
            else // Donor
            {
                var (ok, userId, fullName, username) =
                    await KapwaDataService.LoginDonor(CurrentUser.UserID, CurrentUser.Password);

                if (ok)
                {
                    ErrorVisible = false;
                    UserSession.UserId = userId;
                    UserSession.Username = username;
                    UserSession.FullName = fullName;
                    UserSession.Role = "Donor";
                    NavigationService.Navigate(new View.DonorDashboardWindow(userId));
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                    ErrorVisible = true;
                }
            }
        }
    }
}