using KapwaKuha.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KapwaKuha.ViewModels
{
    public class NeedsWishlistDesignViewModel
    {
        public string Title { get; } = "Blankets for Street Children";
        public string Description { get; } = "We urgently need 50 warm blankets.";
        public string Urgency { get; } = "High";
        public bool IsLow { get; } = false;
        public bool IsMedium { get; } = false;
        public bool IsHigh { get; } = true;
        public ObservableCollection<NeedsPostModel> MyPosts { get; } = new()
        {
            new NeedsPostModel { NeedsPost_ID="NP001", Title="Blankets for Street Children", Urgency="High",   Status="Open",      Post_Date=DateTime.Now.AddDays(-1) },
            new NeedsPostModel { NeedsPost_ID="NP002", Title="School Supplies Kit",          Urgency="Medium", Status="Fulfilled", Post_Date=DateTime.Now.AddDays(-7) },
        };
        public bool IsBusy { get; } = false;
        public bool ErrorVisible { get; } = false;
        public string ErrorMessage { get; } = string.Empty;
    }
}
