using KapwaKuha.ViewModels;
using KapwaKuha.Services;
using System.Windows;

namespace KapwaKuha.View
{
    public partial class DonorLoginWindow : Window
    {
        public DonorLoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel("Donor");
            Loaded += (s, e) => NavigationService.SetCurrent(this);
        }
    }
}