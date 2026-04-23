using System.Windows;
using KapwaKuha.Services;
using KapwaKuha.ViewModels;

namespace KapwaKuha.View
{
    public partial class ClaimTrackerWindow : Window
    {
        public ClaimTrackerWindow(string beneficiaryId)
        {
            InitializeComponent();
            DataContext = new ClaimTrackerViewModel(beneficiaryId);
            Loaded += (s, e) => NavigationService.SetCurrent(this);
        }
    }
}