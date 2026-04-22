// FILE: ChatListViewModel.cs
// Window: ChatListWindow.xaml
// Parallel to AdminChatListViewModel in CarRentals
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class ChatListViewModel : ObservableObject
    {
        private readonly string _myId;
        private readonly string _role;

        public ObservableCollection<ChatUserRow> ChatUsers { get; } = new();

        public ICommand BackCommand { get; }
        public ICommand OpenChatCommand { get; }

        public ChatListViewModel(string myId, string role)
        {
            _myId = myId;
            _role = role;

            BackCommand = new RelayCommand(_ =>
            {
                if (_role == "Donor")
                    NavigationService.Navigate(new View.DonorDashboardWindow(_myId));
                else
                    NavigationService.Navigate(new View.BeneficiaryDashboardWindow(_myId));
            });

            OpenChatCommand = new RelayCommand(user =>
            {
                if (user is ChatUserRow row)
                    NavigationService.Navigate(
                        new View.ChatWindow(_myId, row.UserId, row.DisplayName, _role));
            });

            LoadChats();
        }

        private async void LoadChats()
        {
            if (_role == "Beneficiary")
            {
                var donors = await KapwaDataService.GetChatDonors();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChatUsers.Clear();
                    foreach (var (id, name, last, unread) in donors)
                        ChatUsers.Add(new ChatUserRow
                        {
                            UserId = id,
                            DisplayName = name,
                            LastMessage = last,
                            UnreadCount = unread
                        });
                });
            }
            // Donor view: list of beneficiaries they've chatted with — extend as needed
        }
    }

    public class ChatUserRow
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
    }
}