// FILE: View/AdminSupportChatWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.View
{
    public partial class AdminSupportChatWindow : Window
    {
        private readonly string _userId;   // the logged-in user (donor or bene)
        private readonly string _role;     // "Donor" / "Beneficiary" / "Admin"

        // When opened as ADMIN (browsing support inbox), _adminMode = true and _userId = target user
        private readonly bool _adminMode;

        public AdminSupportChatWindow(string userId, string role, bool adminMode = false)
        {
            InitializeComponent();
            _userId = userId;
            _role = role;
            _adminMode = adminMode;
            _ = LoadMessages();
        }

        private async Task LoadMessages()
        {
            var msgs = await KapwaDataService.GetAdminSupportMessages(_userId);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessagesList.ItemsSource = null;
                var items = new List<SupportChatItem>();
                foreach (var m in msgs)
                {
                    // In admin mode: admin's own messages are "from user" (right-aligned)
                    bool fromMe = _adminMode ? (m.SenderId == "A001") : m.IsFromUser;
                    items.Add(new SupportChatItem
                    {
                        Text = m.Text,
                        TimeLabel = m.Time,
                        IsFromUser = fromMe,
                        HAlignment = fromMe ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                        SenderLabel = fromMe ? "You" : (m.SenderId == "A001" ? "KapwaKuha Support" : m.SenderId),
                    });
                }
                MessagesList.ItemsSource = items;
                ScrollView.ScrollToEnd();
            });
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) await SendMessage();
        }

        private async Task SendMessage()
        {
            string text = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            InputBox.Text = string.Empty;

            // sender is A001 in admin mode, otherwise the logged-in user
            string senderId = _adminMode ? "A001" : _userId;
            string receiverId = _adminMode ? _userId : "A001";

            await KapwaDataService.SaveChatMessage(senderId, receiverId, text);
            await LoadMessages();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    // Simple display model for support chat
    public class SupportChatItem
    {
        public string Text { get; set; } = string.Empty;
        public string TimeLabel { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public HorizontalAlignment HAlignment { get; set; }
        public string SenderLabel { get; set; } = string.Empty;
    }
}