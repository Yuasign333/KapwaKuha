// FILE: ViewModels/ClaimTrackerViewModel.cs
using System;
using System.Collections.Generic;
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
        private readonly string _userId;
        private readonly string _role;

        public ObservableCollection<ClaimModel> Claims { get; } = new();

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        // FIX: FilterCategory filters by CATEGORY_NAME (not Claim_Status)
        // FilterStatus filters by claim status separately
        private string _filterCategory = "All";
        public string FilterCategory
        {
            get => _filterCategory;
            set { _filterCategory = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _filterStatus = "All";
        public string FilterStatus
        {
            get => _filterStatus;
            set { _filterStatus = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private List<ClaimModel> _allClaims = new();

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }
        // Renamed command — beneficiary marks their item as received
        public ICommand ConfirmReceiptCommand { get; }

        public ClaimTrackerViewModel(string userId, string role)
        {
            _userId = userId;
            _role = role;

            BackCommand = new RelayCommand(_ =>
            {
                if (_role == "Donor")
                    NavigationService.Navigate(new View.DonorDashboardWindow(_userId));
                else
                    NavigationService.Navigate(new View.BeneficiaryDashboardWindow(_userId));
            });

            RefreshCommand = new AsyncRelayCommand(async _ => await LoadAsync());

            ConfirmReceiptCommand = new AsyncRelayCommand(async param =>
            {
                if (param is not ClaimModel c) return;

                if (c.Claim_Status == "Released")
                {
                    MessageBox.Show("✅ This item is already marked as received.",
                        "Already Done", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Confirm that you received \"{c.Item_Name}\"?\n\n" +
                    "This will mark the claim as complete in the system.",
                    "Confirm Receipt",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    await KapwaDataService.UpdateClaimStatus(c.Claim_ID, "Released");

                    // Update in-memory immediately for instant UI feedback
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        c.Claim_Status = "Released";
                    });

                    MessageBox.Show("✅ Item marked as received! Thank you.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Full reload to sync with DB
                    await LoadAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Update failed: " + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            _ = LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                List<ClaimModel> claims;
                if (_role == "Donor")
                    claims = await KapwaDataService.GetAllClaimsForDonor(_userId);
                else
                    claims = await KapwaDataService.GetClaimsByBeneficiary(_userId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _allClaims = claims;
                    ApplyFilter();
                });
            }
            catch { }
        }

        private void ApplyFilter()
        {
            Claims.Clear();
            var q = _searchText?.Trim().ToLower() ?? string.Empty;

            foreach (var c in _allClaims)
            {
                bool matchSearch = string.IsNullOrEmpty(q) ||
                                   (c.Item_Name?.ToLower().Contains(q) ?? false) ||
                                   (c.Beneficiary_Name?.ToLower().Contains(q) ?? false) ||
                                   (c.Claim_ID?.ToLower().Contains(q) ?? false);

                // Filter by Category_Name (Fix 3 — category filter)
                bool matchCat = _filterCategory == "All" ||
                                string.Equals(c.Category_Name, _filterCategory,
                                    StringComparison.OrdinalIgnoreCase);

                // Separate status filter
                bool matchStatus = _filterStatus == "All" ||
                                   c.Claim_Status == _filterStatus;

                if (matchSearch && matchCat && matchStatus) Claims.Add(c);
            }
            StatusMessage = $"{Claims.Count} claim(s) shown.";
        }
    }
}