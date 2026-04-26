// FILE: BeneficiaryDashboardViewModel.cs
// Window: BeneficiaryDashboardWindow.xaml
// Parallel to CustomerDashboardViewModel in CarRentals
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class BeneficiaryDashboardViewModel : ObservableObject
    {
        private readonly string _beneficiaryId;

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
        public ICommand ClaimTrackerCommand { get; }
        public ICommand BrowseItemsCommand { get; }
        public ICommand NeedsWishlistCommand { get; }
        public ICommand ChatCommand { get; }
        public ICommand LogoutCommand { get; }

        public BeneficiaryDashboardViewModel(string beneficiaryId)
        {
            _beneficiaryId = beneficiaryId;
            WelcomeText = $"Welcome, {UserSession.FullName}!";
            UserLabel = $"Beneficiary: {UserSession.UserId}";

            HamburgerCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);

            ClaimTrackerCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ClaimTrackerWindow(_beneficiaryId)));

            BrowseItemsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseItemsWindow(_beneficiaryId)));

            NeedsWishlistCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.NeedsWishlistWindow(_beneficiaryId)));

            ChatCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ChatListWindow(_beneficiaryId, "Beneficiary")));

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
        public class BeneficiaryDashboardDesignViewModel : ObservableObject
        {
            public string WelcomeText { get; } = "Welcome, Ana Reyes!";
            public string UserLabel { get; } = "Beneficiary: B001";
            public bool IsSidebarOpen { get; } = false;
            public ICommand HamburgerCommand { get; } = new RelayCommand(_ => { });
            public ICommand ClaimTrackerCommand { get; } = new RelayCommand(_ => { });
            public ICommand BrowseItemsCommand { get; } = new RelayCommand(_ => { });
            public ICommand NeedsWishlistCommand { get; } = new RelayCommand(_ => { });
            public ICommand ChatCommand { get; } = new RelayCommand(_ => { });
            public ICommand LogoutCommand { get; } = new RelayCommand(_ => { });
        }
    }
}