// FILE: Models/BeneficiaryModel.cs
using KapwaKuha.ViewModels;

namespace KapwaKuha.Models
{
    public class BeneficiaryModel : ObservableObject
    {
        public string Beneficiary_ID { get; set; } = string.Empty;

        // DB has Beneficiary_FullName as a single column — no FName/LName split
        public string Beneficiary_FullName { get; set; } = string.Empty;

        // Kept for backward compat — registration form may still split these
        // They feed into Beneficiary_FullName before saving
        public string Beneficiary_FName { get; set; } = string.Empty;
        public string Beneficiary_LName { get; set; } = string.Empty;

        public System.DateTime? Beneficiary_Birthdate { get; set; }
        public string Beneficiary_Sex { get; set; } = string.Empty;
        public string Beneficiary_Contact { get; set; } = string.Empty;
        public string Beneficiary_Username { get; set; } = string.Empty;
        public string Beneficiary_Password { get; set; } = string.Empty;

        private string _status = "Active";
        public string Beneficiaries_Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string Organization_ID { get; set; } = string.Empty;
        public string Organization_Name { get; set; } = string.Empty;

        public string SecurityQuestion { get; set; } = "What is your pet name?";
        public string SecurityAnswer { get; set; } = string.Empty;

        // Used by ComboBox DisplayMemberPath in PostItemWindow
        public string DisplayName =>
            $"{(string.IsNullOrWhiteSpace(Beneficiary_FullName) ? $"{Beneficiary_FName} {Beneficiary_LName}".Trim() : Beneficiary_FullName)} — {Organization_Name}";
    }
}