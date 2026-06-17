// FILE: View/UserProfileWindow.xaml.cs
using KapwaKuha.ViewModels;
using System.Windows;

namespace KapwaKuha.View
{
    public partial class UserProfileWindow : Window
    {
        public UserProfileWindow(string targetId, string viewerId, string viewerRole)
        {
            InitializeComponent();
            var vm = new UserProfileViewModel(targetId, viewerId, viewerRole);
            vm.OnCloseRequested = () => Close();
            DataContext = vm;
        }
        private void ProofImagePreview_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not System.Windows.Controls.Border border) return;
            string? path = border.Tag as string;
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return;

            var popup = new Window
            {
                Title = "Proof Image Preview",
                Width = 860,
                Height = 680,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.CanResize,
                Background = System.Windows.Media.Brushes.Black,
                ShowInTaskbar = false
            };
            var grid = new System.Windows.Controls.Grid();
            grid.Children.Add(new System.Windows.Controls.Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(path)),
                Stretch = System.Windows.Media.Stretch.Uniform,
                Margin = new Thickness(8)
            });
            var hint = new System.Windows.Controls.TextBlock
            {
                Text = "Click anywhere to close",
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(180, 255, 255, 255)),
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 12)
            };
            grid.Children.Add(hint);
            popup.Content = grid;
            popup.MouseLeftButtonDown += (s, _) => popup.Close();
            popup.ShowDialog();
        }
    }
}