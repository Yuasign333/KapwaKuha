// FILE: ItemModel.cs 
// DB Table: Items
// ERD: Strong Entity — provider asset (parallel to CarModel)
// Status lifecycle: "Available" → "Claimed" | "Reserved"
using System;
using KapwaKuha.ViewModels;

namespace KapwaKuha.Models
{
    public class ItemModel : ObservableObject
    {
        public string Item_ID { get; set; } = string.Empty;
        public string Item_Name { get; set; } = string.Empty;
        public string Item_Condition { get; set; } = "Good";

        private string _itemStatus = "Available";
        public string Item_Status
        {
            get => _itemStatus;
            set
            {
                _itemStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(StatusBadgeBackground));
                OnPropertyChanged(nameof(StatusBadgeColor));
            }
        }

        public DateTime Date_Found { get; set; } = DateTime.Now;

        public string Donor_ID { get; set; } = string.Empty;
        public string Donor_Name { get; set; } = string.Empty;
        public string Category_ID { get; set; } = string.Empty;
        public string Category_Name { get; set; } = string.Empty;

        // Post type — "DirectTarget" or "GeneralPost"
        public string PostType { get; set; } = "GeneralPost";

        // For Direct Target — target beneficiary
        public string TargetBeneficiary_ID { get; set; } = string.Empty;

        // Computed
        public int StorageDays => (DateTime.Now - Date_Found).Days;
        public string StorageDaysDisplay =>
            StorageDays == 0 ? "Posted today" :
            StorageDays == 1 ? "1 day posted" :
            $"{StorageDays} days posted";

        // Status badge colors per doc Section 9.3
        public string StatusBadgeBackground => Item_Status switch
        {
            "Available" => "#EBF7FB",
            "Claimed" => "#E8F4F0",
            "Reserved" => "#FFF8E6",
            _ => "#F5F5F5"
        };
        public string StatusBadgeColor => Item_Status switch
        {
            "Available" => "#0077B6",
            "Claimed" => "#2DC653",
            "Reserved" => "#B8860B",
            _ => "#9E9E9E"
        };

        // Legacy single hex color for simple bindings
        public string StatusColor => Item_Status switch
        {
            "Available" => "#00B4D8",
            "Claimed" => "#9E9E9E",
            "Reserved" => "#FFA500",
            _ => "#9E9E9E"
        };
    }
}