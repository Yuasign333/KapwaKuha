// FILE: DonorDashboardViewModel.cs
// Window: DonorDashboardWindow.xaml
// Parallel to AdminDashboardViewModel in CarRentals
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class DonorDashboardViewModel : ObservableObject
    {
        private readonly string _donorId;

        private bool _isSidebarOpen;
        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set { _isSidebarOpen = value; OnPropertyChanged(); OnPropertyChanged(nameof(SidebarWidth)); }
        }
        public GridLength SidebarWidth => IsSidebarOpen ? new GridLength(220) : new GridLength(0);

        public string WelcomeText { get; }
        public string UserLabel { get; }

        public ICommand HamburgerCommand { get; }
        public ICommand PostItemCommand { get; }
        public ICommand MyImpactCommand { get; }
        public ICommand HighPriorityNeedsCommand { get; }
        public ICommand ActiveListingsCommand { get; }
        public ICommand ChatCommand { get; }
        public ICommand LogoutCommand { get; }

        public DonorDashboardViewModel(string donorId)
        {
            _donorId = donorId;
            WelcomeText = $"Welcome back, {UserSession.FullName}!";
            UserLabel = $"Donor: {UserSession.Username}";

            HamburgerCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);

            PostItemCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.PostItemWindow(_donorId)));

            MyImpactCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.MyImpactWindow(_donorId)));

            HighPriorityNeedsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.HighPriorityNeedsWindow(_donorId)));

            ActiveListingsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ActiveListingsWindow(_donorId)));

            ChatCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ChatListWindow(_donorId, "Donor")));

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