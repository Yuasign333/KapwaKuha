// FILE: ItemModel.cs
// DB Table: Items
// ERD: Strong Entity — provider asset (parallel to CarModel)
// Status lifecycle: "Lost" → "Found" → "Claimed"

using System;
using KapwaKuha.ViewModels;

namespace KapwaKuha.Models
{
    public class ItemModel : ObservableObject
    {
        // ── Primary Key ───────────────────────────────────────────────────────
        public string Item_ID { get; set; } = string.Empty;

        // ── Attributes ────────────────────────────────────────────────────────
        public string Item_Name { get; set; } = string.Empty;
        public string Item_Condition { get; set; } = "Unknown";

        private string _itemStatus = "Lost";
        public string Item_Status
        {
            get => _itemStatus;
            set
            {
                _itemStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public DateTime Date_Found { get; set; } = DateTime.Now;

        // ── Foreign Keys (with joined display names) ──────────────────────────
        public string Donor_ID { get; set; } = string.Empty;
        public string Donor_Name { get; set; } = string.Empty;
        public string Category_ID { get; set; } = string.Empty;
        public string Category_Name { get; set; } = string.Empty;

        // ── Computed ──────────────────────────────────────────────────────────
        public int StorageDays => (DateTime.Now - Date_Found).Days;
        public string StorageDaysDisplay =>
            StorageDays == 0 ? "Reported today" :
            StorageDays == 1 ? "1 day in storage" :
            $"{StorageDays} days in storage";

        // StatusBadge color — bound in XAML DataGrid column
        public string StatusColor => Item_Status switch
        {
            "Found" => "#00B4D8",
            "Claimed" => "#9E9E9E",
            _ => "#E63946"
        };
    }
}