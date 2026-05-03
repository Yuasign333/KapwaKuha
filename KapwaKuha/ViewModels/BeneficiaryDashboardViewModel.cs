// FILE: ViewModels/BeneficiaryDashboardViewModel.cs
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
            set { _isSidebarOpen = value; OnPropertyChanged(); }
        }

        public string WelcomeText { get; }
        public string UserLabel { get; }

        public ICommand HamburgerCommand { get; }
        public ICommand ClaimTrackerCommand { get; }
        public ICommand BrowseItemsCommand { get; }
        public ICommand NeedsWishlistCommand { get; }
        public ICommand ChatCommand { get; }
        public ICommand LogoutCommand { get; }

        public ICommand MyAccountCommand { get; }

        public BeneficiaryDashboardViewModel(string beneficiaryId)
        {
            _beneficiaryId = beneficiaryId;
            WelcomeText = $"Welcome, {UserSession.FullName}!";
            UserLabel = $"Beneficiary: {UserSession.UserId}";

            HamburgerCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);

            // Beneficiary sees only their own claims — pass "Beneficiary" role
            ClaimTrackerCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ClaimTrackerWindow(_beneficiaryId, "Beneficiary")));

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

            MyAccountCommand = new RelayCommand(_ =>
        NavigationService.Navigate(new View.BeneficiaryClaimTrackerWindow(_beneficiaryId)));
            ;
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