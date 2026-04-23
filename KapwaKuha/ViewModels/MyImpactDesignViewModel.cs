using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KapwaKuha.ViewModels
{
    public class MyImpactDesignViewModel
    {
        public string DonorName { get; } = "Donor: Juan Dela Cruz";
        public int TotalDonated { get; } = 12;
        public int TotalClaimed { get; } = 9;
        public int ActiveItems { get; } = 3;
        public double AvgStorageDays { get; } = 4.5;
        public string AvgStorageDisplay { get; } = "4.5 days avg. to claim";
    }
}
