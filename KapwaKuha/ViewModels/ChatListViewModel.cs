// FILE: ChatListViewModel.cs
// Window: ChatListWindow.xaml
// Parallel to AdminChatListViewModel in CarRentals
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
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
                // GetChatDonors() returns List<DonorModel>
                var donors = await KapwaDataService.GetChatDonors();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChatUsers.Clear();
                    foreach (var d in donors)
                        ChatUsers.Add(new ChatUserRow
                        {
                            UserId = d.Donor_ID,
                            DisplayName = d.Donor_FullName,
                            LastMessage = "", // populated on demand
                            UnreadCount = 0
                        });
                });
            }
            // Donor view: list of beneficiaries they've chatted with (extend as needed)
        }
    }

    public class ChatUserRow : ObservableObject
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;

        private int _unreadCount;
        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                _unreadCount = value;
                OnPropertyChanged();
                // Recompute Visibility whenever count changes
                OnPropertyChanged(nameof(HasUnread));
            }
        }

        /// <summary>
        /// Visibility-typed property for XAML badge binding.
        /// Visible when UnreadCount > 0, Collapsed otherwise.
        /// </summary>
        public Visibility HasUnread =>
            _unreadCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}