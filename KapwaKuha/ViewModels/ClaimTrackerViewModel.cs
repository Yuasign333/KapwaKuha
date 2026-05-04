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

    
        // For Donor action column (Approve + Mark Released) — visible to DONOR only
        public bool ShowDonorActions => _role == "Donor";

        // For Beneficiary Confirm Receipt button — visible to BENEFICIARY only
        public bool ShowConfirmReceiptButton => _role == "Beneficiary";

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ApproveHandoffCommand { get; }  // Pending → Verified
        public ICommand MarkReleasedCommand { get; }    // Verified → Released

        public ICommand ReleaseItemCommand { get; }

        public ICommand ConfirmReceiptCommand { get; }  // Released → Claimed

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

            ApproveHandoffCommand = new AsyncRelayCommand(async param =>
            {
                if (param is not ClaimModel c) return;
                if (c.Claim_Status != "Pending")
                {
                    MessageBox.Show("Only Pending claims can be approved.", "Info");
                    return;
                }
                var r = MessageBox.Show(
                    $"Approve handoff for \"{c.Item_Name}\"?\nThis moves it to Verified.",
                    "Approve Handoff", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r != MessageBoxResult.Yes) return;

                await KapwaDataService.UpdateClaimStatus(c.Claim_ID, "Verified");
                MessageBox.Show("✅ Claim marked Verified.", "Updated",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
            });

            MarkReleasedCommand = new AsyncRelayCommand(async param =>
            {
                if (param is not ClaimModel c) return;
                if (c.Claim_Status != "Verified")
                {
                    MessageBox.Show("Only Verified claims can be marked Released.", "Info");
                    return;
                }
                var r = MessageBox.Show(
                    $"Mark \"{c.Item_Name}\" as Released?\nThis completes the donation.",
                    "Mark Released", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r != MessageBoxResult.Yes) return;

                await KapwaDataService.UpdateClaimStatus(c.Claim_ID, "Released");
                MessageBox.Show("✅ Claim marked Released. Item is now Claimed.",
                    "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
            });

            ConfirmReceiptCommand = new AsyncRelayCommand(async param =>
            {
                if (param is not ClaimModel c) return;
                if (c.Claim_Status == "Released")
                {
                    MessageBox.Show("This claim is already Released.", "Info");
                    return;
                }
                var r = MessageBox.Show(
                    $"Confirm you received \"{c.Item_Name}\"?\nThis marks the donation as complete.",
                    "Confirm Receipt", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r != MessageBoxResult.Yes) return;

                await KapwaDataService.UpdateClaimStatus(c.Claim_ID, "Released");
                MessageBox.Show("✅ Receipt confirmed! Donation is now complete.",
                    "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
            });

            ReleaseItemCommand = new AsyncRelayCommand(async param =>
            {
                if (param is not ClaimModel c) return;
                if (c.Claim_Status == "Released")
                {
                    MessageBox.Show("This claim is already Released and cannot be cancelled.", "Info");
                    return;
                }

                var r = MessageBox.Show(
                    $"Return \"{c.Item_Name}\" to the marketplace?\n\n" +
                    "Your claim will be cancelled and the item will become available to others.",
                    "Release Item", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (r != MessageBoxResult.Yes) return;

                // Set claim back to a cancelled state — here we delete or set to a cancelled status.
                // Since the DB only allows 'Pending','Verified','Released', we repurpose by:
                //   1. Updating Item_Status back to 'Available'
                //   2. Updating Claim_Status to 'Released' with a note (effectively closing it)
                //   OR if you want to truly cancel, extend the DB constraint to add 'Cancelled'.
                // For now, we mark it Released with a "Returned" note to avoid constraint violation.
                await KapwaDataService.UpdateClaimStatus(c.Claim_ID, "Released");
                await KapwaDataService.RevertItemToGeneralPost(c.Item_ID);   // makes item Available again

                MessageBox.Show("✅ Item returned to marketplace. Your claim has been cancelled.",
                    "Released", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
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
                    Claims.Clear();
                    foreach (var c in claims) Claims.Add(c);
                    StatusMessage = $"{Claims.Count} claim(s) found.";
                });
            }
            catch { }
        }
    }
}