// FILE: ViewModels/BeneficiaryDashboardViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    // NOTE: DashboardChatRow is defined in DonorDashboardViewModel.cs — do NOT redefine it here.

    public class BeneficiaryDashboardViewModel : ObservableObject
    {
        private readonly string _beneficiaryId;

        // Sidebar — starts OPEN by default
        private bool _isSidebarOpen = true;
        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set
            {
                _isSidebarOpen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MessagesColumnWidth));
            }
        }

        public GridLength MessagesColumnWidth =>
            IsSidebarOpen ? new GridLength(240) : new GridLength(300);

        // Identity
        public string WelcomeText { get; }
        public string UserLabel { get; }

        // Profile picture
        private string _profilePicturePath = string.Empty;
        public string ProfilePicturePath
        {
            get => _profilePicturePath;
            set { _profilePicturePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPicture)); }
        }
        public bool HasPicture =>
            !string.IsNullOrEmpty(_profilePicturePath) && System.IO.File.Exists(_profilePicturePath);

        // UI State Flags
        private string _transactionStatus = string.Empty;
        public string TransactionStatus
        {
            get => _transactionStatus;
            set { _transactionStatus = value; OnPropertyChanged(); }
        }

        private bool _hasNoTransactions = true;
        public bool HasNoTransactions
        {
            get => _hasNoTransactions;
            set { _hasNoTransactions = value; OnPropertyChanged(); }
        }

        private bool _hasNoChats = true;
        public bool HasNoChats
        {
            get => _hasNoChats;
            set { _hasNoChats = value; OnPropertyChanged(); }
        }

        private bool _hasNoNeedsPosts = false;
        public bool HasNoNeedsPosts
        {
            get => _hasNoNeedsPosts;
            set { _hasNoNeedsPosts = value; OnPropertyChanged(); }
        }

        // Collections
        public ObservableCollection<TransactionRow> Transactions { get; } = new();
        public ObservableCollection<DashboardChatRow> RecentChats { get; } = new();
        public ObservableCollection<NeedsPostModel> MyNeedsPosts { get; } = new();

        // Commands
        public ICommand HamburgerCommand { get; }
        public ICommand NavigateDashboardCommand { get; }
        public ICommand BrowseItemsCommand { get; }
        public ICommand BrowseByCategoryCommand { get; }
        public ICommand NeedsWishlistCommand { get; }
        public ICommand ClaimTrackerCommand { get; }
        public ICommand ClaimHistoryCommand { get; }
        public ICommand ChatCommand { get; }
        public ICommand MyBAccountCommand { get; }
        public ICommand LogoutCommand { get; }

        public ICommand CategoryCommand { get; }
        public ICommand AddNeedCommand { get; }

        // EditNeedsPostsCommand  → sidebar nav item  → EditNeedsPostUrgencyWindow
        // EditNeedsPostCommand   → per-card button   → EditNeedsPostUrgencyWindow
        public ICommand EditNeedsPostsCommand { get; }
        public ICommand EditNeedsPostCommand { get; }

        public ICommand OpenChatWithCommand { get; }
        public ICommand CarouselLeftCommand { get; }
        public ICommand CarouselRightCommand { get; }

        public BeneficiaryDashboardViewModel(string beneficiaryId)
        {

            _beneficiaryId = beneficiaryId;
            WelcomeText = $"Welcome back, {UserSession.FullName}!";
            UserLabel = $"Beneficiary: {UserSession.UserId}";

            HamburgerCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);
            NavigateDashboardCommand = new RelayCommand(_ => { /* Already on dashboard */ });

            // Category buttons — pass category name as CommandParameter
            BrowseItemsCommand = new RelayCommand(param =>
            {
                string category = param is string s && !string.IsNullOrWhiteSpace(s) ? s : "All";
                NavigationService.Navigate(new View.BrowseItemsWindow(_beneficiaryId, category));
            });

            BrowseByCategoryCommand = new RelayCommand(param =>
            {
                string category = param is string s && !string.IsNullOrWhiteSpace(s) ? s : "All";
                NavigationService.Navigate(new View.BrowseItemsWindow(_beneficiaryId, category));
            });

            CategoryCommand = new RelayCommand(param =>
            {
                string category = param is string s && !string.IsNullOrWhiteSpace(s) ? s : "All";
                NavigationService.Navigate(new View.BrowseItemsWindow(_beneficiaryId, category));
            });

            NeedsWishlistCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.NeedsWishlistWindow(_beneficiaryId)));

            ClaimTrackerCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ClaimTrackerWindow(_beneficiaryId, "Beneficiary")));

            ClaimHistoryCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BeneficiaryClaimHistoryWindow(_beneficiaryId)));

            ChatCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.ChatListWindow(_beneficiaryId, "Beneficiary")));

            MyBAccountCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BeneficiaryProfileWindow(_beneficiaryId)));

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

            AddNeedCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.NeedsWishlistWindow(_beneficiaryId)));

            // Sidebar "Edit My Needs" nav item → resolves org then opens EditNeedsPostUrgencyWindow
            EditNeedsPostsCommand = new RelayCommand(_ =>
            {
                _ = System.Threading.Tasks.Task.Run(async () =>
                {
                    var bene = await KapwaDataService.GetBeneficiaryById(_beneficiaryId);
                    if (bene == null) return;
                    Application.Current.Dispatcher.Invoke(() =>
                        NavigationService.Navigate(
                            new View.EditNeedsPostUrgencyWindow(_beneficiaryId, bene.Organization_ID)));
                });
            });

            // Per-card "Edit Needs" button → resolves org then opens EditNeedsPostUrgencyWindow
            EditNeedsPostCommand = new RelayCommand(param =>
            {
                _ = System.Threading.Tasks.Task.Run(async () =>
                {
                    var bene = await KapwaDataService.GetBeneficiaryById(_beneficiaryId);
                    if (bene == null) return;
                    Application.Current.Dispatcher.Invoke(() =>
                        NavigationService.Navigate(
                            new View.EditNeedsPostUrgencyWindow(_beneficiaryId, bene.Organization_ID)));
                });
            });

            OpenChatWithCommand = new RelayCommand(param =>
            {
                if (param is DashboardChatRow row)
                    NavigationService.Navigate(
                        new View.ChatWindow(_beneficiaryId, row.UserId, row.DisplayName, "Beneficiary"));
            });

            CarouselLeftCommand = new RelayCommand(_ => { });
            CarouselRightCommand = new RelayCommand(_ => { });

            LoadProfileDataAsync();
            LoadBeneficiaryTransactionsAsync();
            LoadNeedsPostsAsync();
            LoadRecentChatsAsync();
        }

        // Data Loading Methods

        private async void LoadProfileDataAsync()
        {
            try
            {
                var bene = await KapwaDataService.GetBeneficiaryById(_beneficiaryId);
                if (bene != null)
                    ProfilePicturePath = bene.ProfilePicturePath ?? string.Empty;
            }
            catch { }
        }

        private async void LoadBeneficiaryTransactionsAsync()
        {
            try
            {
                var txns = await KapwaDataService.GetBeneficiaryTransactionHistory(_beneficiaryId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Transactions.Clear();
                    foreach (var t in txns) Transactions.Add(t);
                    HasNoTransactions = !Transactions.Any();
                    TransactionStatus = Transactions.Any()
                        ? $"{Transactions.Count} item(s) received"
                        : "No received donations yet";
                });
            }
            catch { HasNoTransactions = true; }
        }

        private async void LoadNeedsPostsAsync()
        {
            try
            {
                var bene = await KapwaDataService.GetBeneficiaryById(_beneficiaryId);
                if (bene == null) return;

                var posts = await KapwaDataService.GetNeedsPostsByOrg(bene.Organization_ID);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MyNeedsPosts.Clear();
                    foreach (var p in posts) MyNeedsPosts.Add(p);
                    HasNoNeedsPosts = !MyNeedsPosts.Any();
                });
            }
            catch { HasNoNeedsPosts = true; }
        }

        private async void LoadRecentChatsAsync()
        {
            try
            {
                var donors = await KapwaDataService.GetAllDonorsForChat();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RecentChats.Clear();
                    foreach (var d in donors.Take(5))
                    {
                        RecentChats.Add(new DashboardChatRow
                        {
                            UserId = d.Donor_ID,
                            DisplayName = d.Donor_FullName,
                            LastMessage = string.Empty,
                            UnreadCount = 0
                        });
                    }
                    HasNoChats = !RecentChats.Any();
                });
            }
            catch { HasNoChats = true; }
        }
    }
}