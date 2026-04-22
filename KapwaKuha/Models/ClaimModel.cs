// FILE: ClaimModel.cs
// DB Table: Claims
// ERD: WEAK ENTITY ══ — cannot exist without Item_ID and Beneficiary_ID
// Parallel to RentalModel in CarRentals reference.

using System;
using KapwaKuha.ViewModels;

namespace KapwaKuha.Models
{
    public class ClaimModel : ObservableObject
    {
        public string Claim_ID { get; set; } = string.Empty;
        public string Item_ID { get; set; } = string.Empty;  // NOT NULL — identifying FK
        public string Item_Name { get; set; } = string.Empty;
        public string Beneficiary_ID { get; set; } = string.Empty;  // NOT NULL — identifying FK
        public string Beneficiary_Name { get; set; } = string.Empty;
        public DateTime Claim_Date { get; set; } = DateTime.Now;

        private string _status = "Pending";
        public string Claim_Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string Verification_Notes { get; set; } = string.Empty;

        public string ClaimDateDisplay => Claim_Date.ToString("MMM dd, yyyy  HH:mm");
    }
}