using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;

namespace KapwaKuha.ViewModels
{
    public class BeneficiaryDashboardDesignViewModel
    {
        public string WelcomeText { get; } = "Welcome, Ana Reyes!";
        public string UserLabel { get; } = "Beneficiary: B001";
        public bool IsSidebarOpen { get; } = false;

        public string ProfilePicturePath { get; } = string.Empty;
        public bool HasPicture { get; } = false;

        public ICommand HamburgerCommand { get; } = new RelayCommand(_ => { });
        public ICommand ClaimTrackerCommand { get; } = new RelayCommand(_ => { });
        public ICommand BrowseItemsCommand { get; } = new RelayCommand(_ => { });
        public ICommand NeedsWishlistCommand { get; } = new RelayCommand(_ => { });
        public ICommand ChatCommand { get; } = new RelayCommand(_ => { });
        public ICommand LogoutCommand { get; } = new RelayCommand(_ => { });
        public ICommand MyBAccountCommand { get; } = new RelayCommand(_ => { });
    }
}