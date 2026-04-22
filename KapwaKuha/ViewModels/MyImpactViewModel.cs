// FILE: MyImpactViewModel.cs
// Window: MyImpactWindow.xaml
// Donor impact metrics — parallel to RevenueViewModel
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class MyImpactViewModel : ObservableObject
    {
        private readonly string _donorId;

        private int _totalDonated;
        private int _totalClaimed;
        private int _activeItems;
        private double _avgStorageDays;

        public int TotalDonated { get => _totalDonated; set { _totalDonated = value; OnPropertyChanged(); } }
        public int TotalClaimed { get => _totalClaimed; set { _totalClaimed = value; OnPropertyChanged(); } }
        public int ActiveItems { get => _activeItems; set { _activeItems = value; OnPropertyChanged(); } }
        public double AvgStorageDays { get => _avgStorageDays; set { _avgStorageDays = value; OnPropertyChanged(); } }

        public string DonorName => $"Donor: {UserSession.FullName}";
        public string AvgStorageDisplay => $"{AvgStorageDays:F1} days avg. to claim";

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }

        public MyImpactViewModel(string donorId)
        {
            _donorId = donorId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.DonorDashboardWindow(_donorId)));

            RefreshCommand = new AsyncRelayCommand(async _ => await LoadMetricsAsync());

            LoadMetricsAsync();
        }

        private async System.Threading.Tasks.Task LoadMetricsAsync()
        {
            try
            {
                var (total, claimed, active, avg) =
                    await KapwaDataService.GetImpactMetrics(_donorId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TotalDonated = total;
                    TotalClaimed = claimed;
                    ActiveItems = active;
                    AvgStorageDays = avg;
                    OnPropertyChanged(nameof(AvgStorageDisplay));
                });
            }
            catch { }
        }
    }
}