using System.Windows;
using System.Windows.Controls; // Add this using directive
using KapwaKuha.Services;
using KapwaKuha.ViewModels;

namespace KapwaKuha.View
{
    public partial class SignUpWindow : Window
    {
        // Add this field to hold a reference to OrgCombo
        private ComboBox OrgCombo => (ComboBox)FindName("OrgCombo");

        // SignUpWindow.xaml.cs — must be:
        public SignUpWindow(string role)
        {
            InitializeComponent();
            DataContext = new SignUpViewModel(role);
            Loaded += (s, e) => NavigationService.SetCurrent(this);
        }

        // PasswordBox cannot bind directly — push to ViewModel before executing command
        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SignUpViewModel vm)
            {
                vm.Password = PwBox.Password;
                vm.ConfirmPass = ConfirmPwBox.Password;

                // Wire beneficiary org selection — ValueTuple binding workaround
                if (OrgCombo?.SelectedItem is System.ValueTuple<string, string> org)
                {
                    vm.SelectedOrgId = org.Item1;
                    vm.SelectedOrgName = org.Item2;
                }

                vm.RegisterCommand.Execute(null);
            }
        }
    }
}