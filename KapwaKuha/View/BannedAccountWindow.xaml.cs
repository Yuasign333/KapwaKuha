// FILE: View/BannedAccountWindow.xaml.cs
using System.Windows;

namespace KapwaKuha.View
{
    public partial class BannedAccountWindow : Window
    {
        public string Reason { get; }
        public string StrikesText { get; }

        public BannedAccountWindow(string reason, int strikes)
        {
            Reason = reason;
            StrikesText = $"Strikes accumulated: {strikes} / 3";
            InitializeComponent();
            DataContext = this;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}