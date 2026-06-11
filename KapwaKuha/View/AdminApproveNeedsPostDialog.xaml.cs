// FILE: View/AdminApproveNeedsPostDialog.xaml.cs
using KapwaKuha.Models;
using System.Windows;

namespace KapwaKuha.View
{
    public partial class AdminApproveNeedsPostDialog : Window
    {
        public string ChosenUrgency { get; private set; } = "Medium";

        public AdminApproveNeedsPostDialog(NeedsPostModel post)
        {
            InitializeComponent();
            PostTitleLabel.Text = $"\"{post.Title}\" from {post.Org_Name}  —  submitted as {post.Urgency} urgency";

            // Pre-select the submitted urgency
            switch (post.Urgency)
            {
                case "High": RbHigh.IsChecked = true; break;
                case "Low": RbLow.IsChecked = true; break;
                default: RbMed.IsChecked = true; break;
            }
        }

        private void ApproveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RbHigh.IsChecked == true) ChosenUrgency = "High";
            else if (RbLow.IsChecked == true) ChosenUrgency = "Low";
            else ChosenUrgency = "Medium";
            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}