// FILE: ViewModels/BeneficiaryClaimHistoryViewModel.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class BeneficiaryClaimHistoryViewModel : ObservableObject
    {
        private readonly string _beneficiaryId;

        public ObservableCollection<ClaimModel> Claims { get; } = new();

        private List<ClaimModel> _allClaims = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _filterStatus = "All";
        public string FilterStatus
        {
            get => _filterStatus;
            set { _filterStatus = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _filterCategory = "All";
        public string FilterCategory
        {
            get => _filterCategory;
            set { _filterCategory = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }

        public BeneficiaryClaimHistoryViewModel(string beneficiaryId)
        {
            _beneficiaryId = beneficiaryId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BeneficiaryDashboardWindow(_beneficiaryId)));

            RefreshCommand = new AsyncRelayCommand(async _ => await LoadAsync());

            _ = LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var result = await KapwaDataService.GetClaimHistoryByBeneficiary(_beneficiaryId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _allClaims = result;
                    ApplyFilter();
                });
            }
            catch { }
            finally { IsBusy = false; }
        }

        private void ApplyFilter()
        {
            Claims.Clear();
            var q = _searchText.Trim().ToLower();
            foreach (var c in _allClaims)
            {
                bool matchSearch = string.IsNullOrEmpty(q) ||
                                   c.Item_Name.ToLower().Contains(q) ||
                                   c.Claim_ID.ToLower().Contains(q) ||
                                   c.Category_Name.ToLower().Contains(q);

                bool matchStatus = _filterStatus == "All" ||
                                   c.Claim_Status == _filterStatus;

                bool matchCategory = _filterCategory == "All" ||
                                     c.Category_Name == _filterCategory;

                if (matchSearch && matchStatus && matchCategory) Claims.Add(c);
            }
            StatusMessage = $"{Claims.Count} claim(s) found.";
        }
    }
}