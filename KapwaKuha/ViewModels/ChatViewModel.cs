// FILE: ChatViewModel.cs
// Window: ChatWindow.xaml
// Parallel to ChatViewModel in CarRentals — exactly same pattern
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        private readonly string _myId;
        private readonly string _otherId;
        private readonly string _role;

        public string OtherName { get; }
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        private string _inputText = string.Empty;
        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand BackCommand { get; }
        public ICommand SendCommand { get; }

        public ChatViewModel(string myId, string otherId, string otherName, string role)
        {
            _myId = myId;
            _otherId = otherId;
            _role = role;
            OtherName = otherName;

            BackCommand = new RelayCommand(_ =>
            {
                if (_role == "Donor")
                    NavigationService.Navigate(new View.ChatListWindow(_myId, "Donor"));
                else
                    NavigationService.Navigate(new View.ChatListWindow(_myId, "Beneficiary"));
            });

            SendCommand = new AsyncRelayCommand(async _ =>
            {
                if (string.IsNullOrWhiteSpace(InputText)) return;
                string msg = InputText.Trim();
                InputText = string.Empty;

                await KapwaDataService.SaveChatMessage(_myId, _otherId, msg);
                await LoadMessages();
            });

            LoadMessages();
        }

        private async System.Threading.Tasks.Task LoadMessages()
        {
            try
            {
                var msgs = await KapwaDataService.GetChatMessages(_myId, _otherId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Clear();
                    foreach (var m in msgs) Messages.Add(m);
                });
            }
            catch { }
        }
    }
}