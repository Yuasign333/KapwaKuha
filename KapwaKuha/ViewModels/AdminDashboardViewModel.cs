// FILE: AdminDashboardViewModel.cs  (REVISED)
// Window: AdminDashboardWindow.xaml
// Note: In KapwaKuha the "Admin" role is not used in the doc — kept for dev/test access
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class AdminDashboardViewModel : ObservableObject
    {
        private readonly string _userId;

        private bool _isSidebarOpen;
        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set { _isSidebarOpen = value; OnPropertyChanged(); OnPropertyChanged(nameof(SidebarWidth)); }
        }
        public GridLength SidebarWidth => IsSidebarOpen ? new GridLength(220) : new GridLength(0);

        public string UserLabel { get; }
        public string WelcomeText { get; }

        public ICommand HamburgerCommand { get; }
        public ICommand ItemsCommand { get; }
        public ICommand ClaimCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminDashboardViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";
            WelcomeText = $"Welcome back, {userId}!";

            HamburgerCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);

            ItemsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ItemsWindow(_userId)));

            ClaimCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ClaimProcessWindow(_userId)));

            LogoutCommand = new RelayCommand(_ =>
            {
                var r = MessageBox.Show("Log out?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes)
                {
                    UserSession.Clear();
                    NavigationService.Navigate(new View.ChooseRoleWindow());
                }
            });
        }
    }
}