using KapwaKuha.ViewModels;
using System.Collections.ObjectModel;

public class PostItemDesignViewModel
{
    public string DonorLabel { get; } = "Donor: juandc";
    public string ItemName { get; } = "Assorted School Supplies";
    public string SelectedCategory { get; } = "School Supplies";
    public string SelectedCondition { get; } = "New";
    public bool IsGeneralPost { get; } = true;
    public bool IsDirectTarget { get; } = false;
    public ObservableCollection<string> Categories { get; } =
        new() { "Clothing", "Food", "Electronics", "Medicine", "School Supplies" };
    public ObservableCollection<string> Conditions { get; } =
        new() { "New", "Good", "Fair", "Poor" };
    public ObservableCollection<BeneficiaryRow> Beneficiaries { get; } = new()
        {
            new BeneficiaryRow { Id="B001", DisplayName="Ana Reyes — Barangay San Jose" },
            new BeneficiaryRow { Id="B002", DisplayName="Carlo Santos — SISC Student Welfare" },
        };
    public bool IsBusy { get; } = false;
    public bool ErrorVisible { get; } = false;
    public string ErrorMessage { get; } = string.Empty;
}