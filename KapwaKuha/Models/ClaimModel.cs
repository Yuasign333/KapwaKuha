// FILE: Models/ClaimModel.cs
using System;
using KapwaKuha.ViewModels;

namespace KapwaKuha.Models
{
    public class ClaimModel : ObservableObject
    {
        public string Claim_ID { get; set; } = string.Empty;
        public string Item_ID { get; set; } = string.Empty;
        public string Item_Name { get; set; } = string.Empty;
        public string Item_ImagePath { get; set; } = string.Empty;
        public string Beneficiary_ID { get; set; } = string.Empty;
        public string Beneficiary_Name { get; set; } = string.Empty;
        public DateTime Claim_Date { get; set; } = DateTime.Now;

        public string Category_Name { get; set; } = string.Empty;

        public bool HasItemImage =>
            !string.IsNullOrEmpty(Item_ImagePath) &&
            System.IO.File.Exists(Item_ImagePath);

        private string _status = "Pending";
        public string Claim_Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusBadgeBackground));
                OnPropertyChanged(nameof(StatusBadgeColor));
            }
        }

        public string Verification_Notes { get; set; } = string.Empty;
        public string Handoff_Type { get; set; } = "Pickup";

        public string ClaimDateDisplay => Claim_Date.ToString("MMM dd, yyyy  HH:mm");

        public string StatusBadgeBackground => Claim_Status switch
        {
            "Pending" => "#F0EBFF",
            "Verified" => "#F0FFF4",
            "Released" => "#EAF6FB",
            "Cancelled" => "#FFF0F0",
            _ => "#F5F5F5"
        };
        public string StatusBadgeColor => Claim_Status switch
        {
            "Pending" => "#6B4FA8",
            "Verified" => "#2E7D52",
            "Released" => "#03045E",
            "Cancelled" => "#C0304A",
            _ => "#9E9E9E"
        };
    }
}