// AdminDashboardWindow.xaml.cs
using KapwaKuha.ViewModels;
using System.Windows;
using KapwaKuha.Services;

public partial class AdminDashboardWindow : Window
{
    public AdminDashboardWindow(string userId)
    {
        InitializeComponent();
        DataContext = new AdminDashboardViewModel(userId);
        Loaded += (s, e) => NavigationService.SetCurrent(this);
    }
}