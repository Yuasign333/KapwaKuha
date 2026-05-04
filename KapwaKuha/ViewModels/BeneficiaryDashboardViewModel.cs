// FILE: ViewModels/BeneficiaryDashboardViewModel.cs
using System;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
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

        // --- Profile Picture Properties for the Top Bar Avatar ---
        private string _profilePicturePath = string.Empty;
        public string ProfilePicturePath
        {
            get => _profilePicturePath;
            set
            {
                _profilePicturePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPicture));
            }
        }

        public bool HasPicture =>
            !string.IsNullOrEmpty(_profilePicturePath) && System.IO.File.Exists(_profilePicturePath);


        public ICommand HamburgerCommand { get; }
        public ICommand ClaimTrackerCommand { get; }
        public ICommand BrowseItemsCommand { get; }
        public ICommand NeedsWishlistCommand { get; }
        public ICommand ChatCommand { get; }
        public ICommand LogoutCommand { get; }

        // MUST MATCH YOUR XAML BINDING
        public ICommand MyBAccountCommand { get; }

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

            // FIX 1: Name matches XAML
            // FIX 2: Navigates to the Profile Window, NOT the Claim Tracker
            MyBAccountCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BeneficiaryProfileWindow(_beneficiaryId)));

            // Load the profile picture when the dashboard opens
            LoadProfileDataAsync();
        }

        // Fetch the beneficiary's picture from the database
        private async void LoadProfileDataAsync()
        {
            try
            {
                var bene = await KapwaDataService.GetBeneficiaryById(_beneficiaryId);
                if (bene != null)
                {
                    ProfilePicturePath = bene.ProfilePicturePath ?? string.Empty;
                }
            }
            catch { /* Ignore error silently for dashboard */ }
        }
    }
}