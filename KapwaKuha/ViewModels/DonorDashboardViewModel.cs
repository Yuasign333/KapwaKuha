// FILE: ViewModels/DonorDashboardViewModel.cs
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
            set { _isSidebarOpen = value; OnPropertyChanged(); }
        }

        public string WelcomeText { get; }
        public string UserLabel { get; }

        public ICommand HamburgerCommand { get; }
        public ICommand PostItemCommand { get; }
        public ICommand MyImpactCommand { get; }
        public ICommand HighPriorityNeedsCommand { get; }
        public ICommand ActiveListingsCommand { get; }
        public ICommand ClaimTrackerCommand { get; }
        public ICommand ChatCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand MyAccountCommand { get; }

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

            // Donor sees their items' claims — pass "Donor" role
            ClaimTrackerCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.DonorClaimTrackerWindow(_donorId)));

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

            MyAccountCommand = new RelayCommand(_ =>
    NavigationService.Navigate(new View.DonorProfileWindow(_donorId)));
        }

        public class DonorDashboardDesignViewModel : ObservableObject
        {
            public string WelcomeText { get; } = "Welcome back, Juan Dela Cruz!";
            public string UserLabel { get; } = "Donor: juandc";
            public bool IsSidebarOpen { get; } = false;
            public ICommand HamburgerCommand { get; } = new RelayCommand(_ => { });
            public ICommand PostItemCommand { get; } = new RelayCommand(_ => { });
            public ICommand MyImpactCommand { get; } = new RelayCommand(_ => { });
            public ICommand HighPriorityNeedsCommand { get; } = new RelayCommand(_ => { });
            public ICommand ActiveListingsCommand { get; } = new RelayCommand(_ => { });
            public ICommand ClaimTrackerCommand { get; } = new RelayCommand(_ => { });
            public ICommand ChatCommand { get; } = new RelayCommand(_ => { });
            public ICommand LogoutCommand { get; } = new RelayCommand(_ => { });
        }
    }
}