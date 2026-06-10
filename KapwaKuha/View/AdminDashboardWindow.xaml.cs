using KapwaKuha.ViewModels;
using KapwaKuha.Services;
using System.Windows;

namespace KapwaKuha.View
{
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow(string adminId)
        {
            try
            {
                InitializeComponent();
                DataContext = new AdminDashboardViewModel(adminId);
                Loaded += (s, e) => NavigationService.SetCurrent(this);
            }
           

            catch (Exception ex)
{
                // This catches XAML inflation crashes, missing window resources, and initialization errors!
                MessageBox.Show($"CRITICAL VIEW INFLATION CRASH:\n\n{ex.ToString()}", "Window Init Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}