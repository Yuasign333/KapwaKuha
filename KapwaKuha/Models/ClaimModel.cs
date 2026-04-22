// FILE: ClaimModel.cs  
// DB Table: Claims — WEAK ENTITY
// Cannot exist without Item_ID (NOT NULL) and Beneficiary_ID (NOT NULL)
using System;
using KapwaKuha.ViewModels;

namespace KapwaKuha.Models
{
    public class ClaimModel : ObservableObject
    {
        public string Claim_ID { get; set; } = string.Empty;
        public string Item_ID { get; set; } = string.Empty;       // NOT NULL — identifying FK
        public string Item_Name { get; set; } = string.Empty;
        public string Beneficiary_ID { get; set; } = string.Empty; // NOT NULL — identifying FK
        public string Beneficiary_Name { get; set; } = string.Empty;
        public DateTime Claim_Date { get; set; } = DateTime.Now;

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

        // Badge colors per doc Section 9.3
        public string StatusBadgeBackground => Claim_Status switch
        {
            "Pending" => "#F0EBFF",
            "Verified" => "#F0FFF4",
            "Released" => "#EAF6FB",
            _ => "#F5F5F5"
        };
        public string StatusBadgeColor => Claim_Status switch
        {
            "Pending" => "#6B4FA8",
            "Verified" => "#2E7D52",
            "Released" => "#03045E",
            _ => "#9E9E9E"
        };
    }
}