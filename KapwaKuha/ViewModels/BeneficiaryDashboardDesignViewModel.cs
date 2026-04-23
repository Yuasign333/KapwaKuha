using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KapwaKuha.ViewModels
{
    public class BeneficiaryDashboardDesignViewModel
    {
        public string WelcomeText { get; } = "Welcome, Ana Reyes!";
        public string UserLabel { get; } = "Beneficiary: B001";
        public bool IsSidebarOpen { get; } = true;
        public GridLength SidebarWidth { get; } = new GridLength(220);
    }
}
