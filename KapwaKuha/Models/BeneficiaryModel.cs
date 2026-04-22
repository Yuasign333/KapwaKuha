// FILE: BeneficiaryModel.cs
// DB Table: Beneficiaries
// ERD: Strong Entity — parallel to CustomerModel in CarRentals
using KapwaKuha.ViewModels;

namespace KapwaKuha.Models
{
    public class BeneficiaryModel : ObservableObject
    {
        public string Beneficiary_ID { get; set; } = string.Empty;
        public string Beneficiary_FName { get; set; } = string.Empty;
        public string Beneficiary_LName { get; set; } = string.Empty;
        public string Beneficiary_FullName => $"{Beneficiary_FName} {Beneficiary_LName}".Trim();

        public System.DateTime? Beneficiary_Birthdate { get; set; }
        public string Beneficiary_Sex { get; set; } = string.Empty;
        public string Beneficiary_Contact { get; set; } = string.Empty;

        private string _status = "Active";
        public string Beneficiaries_Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string Organization_ID { get; set; } = string.Empty;
        public string Organization_Name { get; set; } = string.Empty;

        // Display name for ComboBox: "Ana Reyes — Barangay San Jose"
        public string DisplayName => $"{Beneficiary_FullName} — {Organization_Name}";
    }
}