// FILE: View/AdminRejectReasonDialog.xaml.cs
using System.Windows;

namespace KapwaKuha.View
{
    public partial class AdminRejectReasonDialog : Window
    {
        public string Reason => ReasonBox.Text.Trim();

        public AdminRejectReasonDialog(string title, string subtitle)
        {
            InitializeComponent();
            TitleLabel.Text = title;
            SubtitleLabel.Text = subtitle;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}