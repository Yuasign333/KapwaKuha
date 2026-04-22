// FILE: ClaimItemViewModel.cs
// Window: ClaimItemWindow.xaml
// Handles the claim flow: HandoffType + confirmation — parallel to ProcessReturnViewModel
using System;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class ClaimItemViewModel : ObservableObject
    {
        private readonly string _beneficiaryId;
        public ItemModel Item { get; }

        private string _handoffType = "Pickup";
        private string _location = string.Empty;
        private string _eventName = string.Empty;
        private bool _isBusy;
        private string _errorMessage = string.Empty;
        private bool _errorVisible;

        public string HandoffType
        {
            get => _handoffType;
            set
            {
                _handoffType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPickup));
                OnPropertyChanged(nameof(IsDelivery));
                OnPropertyChanged(nameof(IsDonationDrive));
            }
        }
        public bool IsPickup { get => _handoffType == "Pickup"; set { if (value) HandoffType = "Pickup"; } }
        public bool IsDelivery { get => _handoffType == "Delivery"; set { if (value) HandoffType = "Delivery"; } }
        public bool IsDonationDrive { get => _handoffType == "Donation Drive"; set { if (value) HandoffType = "Donation Drive"; } }

        public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }
        public string EventName { get => _eventName; set { _eventName = value; OnPropertyChanged(); } }
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }
        public bool ErrorVisible { get => _errorVisible; set { _errorVisible = value; OnPropertyChanged(); } }

        public ICommand BackCommand { get; }
        public ICommand ConfirmClaimCommand { get; }

        public ClaimItemViewModel(string beneficiaryId, ItemModel item)
        {
            _beneficiaryId = beneficiaryId;
            Item = item;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseItemsWindow(_beneficiaryId)));

            ConfirmClaimCommand = new AsyncRelayCommand(async _ =>
            {
                ErrorVisible = false;

                if (string.IsNullOrWhiteSpace(Location))
                { ErrorMessage = "Please enter a pickup/delivery location."; ErrorVisible = true; return; }

                var confirm = MessageBox.Show(
                    $"Claim this item?\n\nItem: {Item.Item_Name}\n" +
                    $"Handoff: {HandoffType}\nLocation: {Location}",
                    "Confirm Claim", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    IsBusy = true;
                    string claimId = await KapwaDataService.GetNextClaimId();
                    var claim = new ClaimModel
                    {
                        Claim_ID = claimId,
                        Item_ID = Item.Item_ID,
                        Item_Name = Item.Item_Name,
                        Beneficiary_ID = _beneficiaryId,
                        Beneficiary_Name = UserSession.FullName,
                        Claim_Date = DateTime.Now,
                        Claim_Status = "Pending",
                        Handoff_Type = HandoffType,
                        Verification_Notes = $"Location: {Location} | Event: {EventName}"
                    };

                    await KapwaDataService.SaveClaim(claim);
                    await KapwaDataService.UpdateItemStatus(Item.Item_ID, "Claimed");
                    KapwaDataService.GenerateClaimReport(claim);

                    MessageBox.Show($"✅ Claimed! Your Claim ID: {claimId}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.Navigate(new View.BeneficiaryDashboardWindow(_beneficiaryId));
                }
                catch { }
                finally { IsBusy = false; }
            });
        }
    }
}