using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KapwaKuha.ViewModels
{
    public class DonorDashboardDesignViewModel
    {
        public string WelcomeText { get; } = "Welcome back, Juan Dela Cruz!";
        public string UserLabel { get; } = "Donor: juandc";
        public bool IsSidebarOpen { get; } = true;
        public GridLength SidebarWidth { get; } = new GridLength(220);
    }
}
