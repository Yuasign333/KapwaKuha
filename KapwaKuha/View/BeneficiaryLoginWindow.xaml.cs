using KapwaKuha.ViewModels;
using KapwaKuha.Services;
using System.Windows;

namespace KapwaKuha.View
{
    public partial class BeneficiaryLoginWindow : Window
    {
        public BeneficiaryLoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel("Beneficiary");
            Loaded += (s, e) => NavigationService.SetCurrent(this);
        }
    }
}