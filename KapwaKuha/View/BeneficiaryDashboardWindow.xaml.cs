using KapwaKuha.ViewModels;
using KapwaKuha.Services;
using System.Windows;

namespace KapwaKuha.View
{
    public partial class BeneficiaryDashboardWindow : Window
    {
        public BeneficiaryDashboardWindow(string beneficiaryId)
        {
            InitializeComponent();
            var vm = new BeneficiaryDashboardViewModel(beneficiaryId);
            DataContext = vm;

            // Wire carousel arrow buttons to the named ScrollViewer
            vm.CarouselScrollRequested += delta =>
            {
                NeedsCarouselScroller.ScrollToHorizontalOffset(
                    NeedsCarouselScroller.HorizontalOffset + delta);
            };

            Loaded += (s, e) => NavigationService.SetCurrent(this);
        }
        private void SupportBtn_Click(object sender, RoutedEventArgs e)
        {
            var win = new View.AdminSupportChatWindow(
                UserSession.UserId,
                UserSession.Role);
            win.ShowDialog();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            (DataContext as DonorDashboardViewModel)?.NotifVM.StopPolling();
        }
    }

}