using KapwaKuha.ViewModels;
using KapwaKuha.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
                MessageBox.Show($"CRITICAL VIEW INFLATION CRASH:\n\n{ex}", "Window Init Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // ── Logout ────────────────────────────────────────────────────────────
        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            UserSession.Clear();
            var login = new ChooseRoleWindow();
            login.Show();
            Close();
        }

        // ── Report image — fullscreen popup ───────────────────────────────────
        private void ReportImage_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border border) return;
            string? path = border.Tag as string;
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return;
            ShowImagePopup(path, "Report Proof Image");
        }

        // ── Item image zoom ───────────────────────────────────────────────────
        private void ItemImage_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border border) return;
            string? path = border.Tag as string;
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return;
            ShowImagePopup(path, "Item Image Preview");
        }

        // ── Needs post image zoom (ZoomImageCommand calls this via code-behind) ─
        private void NeedsImage_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border border) return;
            string? path = border.Tag as string;
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return;
            ShowImagePopup(path, "Needs Post Image");
        }

        // ── Shared fullscreen image popup ─────────────────────────────────────
        private void ShowImagePopup(string path, string title)
        {
            var popup = new Window
            {
                Title = title,
                Width = 800,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.CanResize,
                Background = Brushes.Black,
                ShowInTaskbar = false
            };

            var grid = new Grid();

            var img = new Image
            {
                Source = new BitmapImage(new Uri(path)),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(8)
            };
            grid.Children.Add(img);

            // Close hint overlay
            var hint = new TextBlock
            {
                Text = "Click anywhere to close",
                Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
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

        // ── Ban dialog — shows reason input before banning ────────────────────
        public static string? ShowBanReasonDialog(string reportedName, Window owner)
        {
            var dialog = new Window
            {
                Title = $"Ban User — {reportedName}",
                Width = 480,
                Height = 260,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = owner,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252))
            };

            var root = new StackPanel { Margin = new Thickness(24) };

            root.Children.Add(new TextBlock
            {
                Text = $"You are about to permanently ban:",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                Margin = new Thickness(0, 0, 0, 4)
            });
            root.Children.Add(new TextBlock
            {
                Text = reportedName,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(10, 37, 64)),
                Margin = new Thickness(0, 0, 0, 16)
            });
            root.Children.Add(new TextBlock
            {
                Text = "BAN REASON (required)",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Margin = new Thickness(0, 0, 0, 6)
            });

            var reasonBox = new TextBox
            {
                Height = 64,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                FontSize = 13,
                Padding = new Thickness(10, 8, 10, 8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                BorderThickness = new Thickness(1),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 0, 16)
            };
            root.Children.Add(reasonBox);

            var btnRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var cancelBtn = new Button
            {
                Content = "Cancel",
                Width = 90,
                Height = 34,
                Margin = new Thickness(0, 0, 10, 0),
                Cursor = Cursors.Hand,
                FontSize = 13
            };

            var banBtn = new Button
            {
                Content = "🚫 Confirm Ban",
                Width = 130,
                Height = 34,
                Background = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            string? result = null;

            cancelBtn.Click += (s, e) => dialog.Close();
            banBtn.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(reasonBox.Text))
                {
                    MessageBox.Show("Please enter a ban reason before confirming.", "Reason Required",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                result = reasonBox.Text.Trim();
                dialog.Close();
            };

            btnRow.Children.Add(cancelBtn);
            btnRow.Children.Add(banBtn);
            root.Children.Add(btnRow);

            dialog.Content = root;
            dialog.ShowDialog();
            return result;
        }

        private void AdminInboxListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) { }
    }
}