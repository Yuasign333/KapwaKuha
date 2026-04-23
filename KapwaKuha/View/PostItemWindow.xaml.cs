using System.Windows;
using KapwaKuha.Services;
using KapwaKuha.ViewModels;

namespace KapwaKuha.View
{
    public partial class PostItemWindow : Window
    {
        public PostItemWindow(string donorId, string prefillTitle = "")
        {
            InitializeComponent();
            var vm = new PostItemViewModel(donorId);
            if (!string.IsNullOrEmpty(prefillTitle)) vm.ItemName = prefillTitle;
            DataContext = vm;
            Loaded += (s, e) => NavigationService.SetCurrent(this);
        }
    }
}