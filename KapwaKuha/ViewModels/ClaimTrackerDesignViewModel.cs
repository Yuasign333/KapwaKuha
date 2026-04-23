using KapwaKuha.Models;
using System.Collections.ObjectModel;

public class ClaimTrackerDesignViewModel
{
    public ObservableCollection<ClaimModel> Claims { get; } = new()
        {
            new ClaimModel { Claim_ID="CL001", Item_Name="School Bag",     Claim_Status="Pending",  Handoff_Type="Pickup",        Claim_Date=DateTime.Now.AddDays(-2) },
            new ClaimModel { Claim_ID="CL002", Item_Name="Winter Blanket", Claim_Status="Verified", Handoff_Type="Delivery",      Claim_Date=DateTime.Now.AddDays(-5) },
            new ClaimModel { Claim_ID="CL003", Item_Name="Vitamins Pack",  Claim_Status="Released", Handoff_Type="Donation Drive", Claim_Date=DateTime.Now.AddDays(-8) },
        };
    public string StatusMessage { get; } = "3 claim(s) found.";
    public bool IsBusy { get; } = false;
}
