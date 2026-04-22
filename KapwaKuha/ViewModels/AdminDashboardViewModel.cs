using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class AdminDashboardViewModel : ObservableObject
    {
        private readonly string _userId;

        // ── Sidebar ───────────────────────────────────────────────────────────
        private bool _isSidebarOpen;
        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set { _isSidebarOpen = value; OnPropertyChanged(); OnPropertyChanged(nameof(SidebarWidth)); }
        }
        public GridLength SidebarWidth => IsSidebarOpen ? new GridLength(220) : new GridLength(0);

        // ── Display ───────────────────────────────────────────────────────────
        public string UserLabel { get; }
        public string WelcomeText { get; }

        // ── Commands ──────────────────────────────────────────────────────────
        public ICommand HamburgerCommand { get; }
        public ICommand ItemsCommand { get; }   // Browse/manage all items  (≈ Fleet)
        public ICommand ClaimCommand { get; }   // Process Claim            (≈ Process Return)
        public ICommand AddItemCommand { get; }   // Register new item        (≈ Add Car)
        public ICommand ImpactCommand { get; }   // Impact dashboard         (≈ Revenue)
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

            AddItemCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AddItemWindow(_userId)));

            ImpactCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ImpactDashboardWindow(_userId)));

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