// FILE: ClaimTrackerViewModel.cs
// Window: ClaimTrackerWindow.xaml
// Loads beneficiary's own claims — parallel to MyRentalsViewModel
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class ClaimTrackerViewModel : ObservableObject
    {
        private readonly string _beneficiaryId;

        public ObservableCollection<ClaimModel> Claims { get; } = new();
        public string StatusMessage { get; private set; } = string.Empty;
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }

        public ClaimTrackerViewModel(string beneficiaryId)
        {
            _beneficiaryId = beneficiaryId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BeneficiaryDashboardWindow(_beneficiaryId)));

            RefreshCommand = new AsyncRelayCommand(async _ => await LoadClaimsAsync());

            LoadClaimsAsync();
        }

        private async System.Threading.Tasks.Task LoadClaimsAsync()
        {
            IsBusy = true;
            try
            {
                var claims = await KapwaDataService.GetClaimsByBeneficiary(_beneficiaryId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Claims.Clear();
                    foreach (var c in claims) Claims.Add(c);
                    StatusMessage = $"{Claims.Count} claim(s) found.";
                    OnPropertyChanged(nameof(StatusMessage));
                });
            }
            catch { }
            finally { IsBusy = false; }
        }
    }
}