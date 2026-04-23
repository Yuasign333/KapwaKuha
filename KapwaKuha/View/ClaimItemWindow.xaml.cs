using System.Windows;
using KapwaKuha.Models;
using KapwaKuha.Services;
using KapwaKuha.ViewModels;

namespace KapwaKuha.View
{
    public partial class ClaimItemWindow : Window
    {
        public ClaimItemWindow(string beneficiaryId, ItemModel item)
        {
            InitializeComponent();
            DataContext = new ClaimItemViewModel(beneficiaryId, item);
            Loaded += (s, e) => NavigationService.SetCurrent(this);
        }
    }
}