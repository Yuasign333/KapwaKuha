using System.Windows; // Required for Application.Current
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class ChooseRoleViewModel : ObservableObject
    {
        public ICommand DonorCommand { get; }
        public ICommand BeneficiaryCommand { get; }
        public ICommand AdminCommand { get; }
        // 1. Declare the ExitCommand property
        public ICommand ExitCommand { get; }

        public ChooseRoleViewModel()
        {
            DonorCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.DonorLoginWindow()));

            BeneficiaryCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BeneficiaryTypeSelectWindow()));

            AdminCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminLoginWindow()));

            // 2. Initialize the ExitCommand to cleanly shutdown the app
            ExitCommand = new RelayCommand(_ =>
            {
                Application.Current.Shutdown();
            });
        }
    }
}