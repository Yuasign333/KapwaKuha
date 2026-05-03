// FILE: View/ClaimTrackerWindow.xaml.cs
using System.Windows;
using KapwaKuha.Services;
using KapwaKuha.ViewModels;

namespace KapwaKuha.View
{
    public partial class ClaimTrackerWindow : Window
    {
        public ClaimTrackerWindow(string userId, string role)
        {
            InitializeComponent();
            DataContext = new ClaimTrackerViewModel(userId, role);
            Loaded += (s, e) => NavigationService.SetCurrent(this);
        }
    }
}