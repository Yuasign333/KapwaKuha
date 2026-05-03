// FILE: ViewModels/ChatViewModel.cs
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;
using System.Linq;

namespace KapwaKuha.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        private readonly string _myId;
        private readonly string _otherId;
        private readonly string _role;

        public string OtherName { get; }

        // Exposed so XAML can AND with IsSystemDirectTarget to hide buttons from donor
        public bool IsBeneficiary => _role == "Beneficiary";

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
        public ICommand SendImageCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand DeclineCommand { get; }

        public event Action? ScrollToBottom;

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

            SendImageCommand = new AsyncRelayCommand(async _ =>
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                    Title = "Select Image to Send"
                };
                if (dlg.ShowDialog() != true) return;
                string msg = $"[IMG]{dlg.FileName}";
                await KapwaDataService.SaveChatMessage(_myId, _otherId, msg);
                await LoadMessages();
            });

            // FIXED: null-safe, checks IsBeneficiary, validates LinkedItemId
            AcceptCommand = new AsyncRelayCommand(async param =>
            {
                // Guard: only beneficiaries can accept
                if (!IsBeneficiary) return;
                if (param is not ChatMessage msg) return;
                if (string.IsNullOrEmpty(msg.LinkedItemId))
                {
                    MessageBox.Show("No item linked to this message.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Accept this donated item?\n\nItem ID: {msg.LinkedItemId}\n\n" +
                    "This will create a claim in your Claim Tracker.",
                    "Accept Donation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    IsBusy = true;
                    string claimId = await KapwaDataService.GetNextClaimId();

                    var claim = new ClaimModel
                    {
                        Claim_ID = claimId,
                        Item_ID = msg.LinkedItemId,   // safe — checked above
                        Item_Name = "",
                        Beneficiary_ID = _myId,
                        Beneficiary_Name = UserSession.FullName,
                        Claim_Date = System.DateTime.Now,
                        Claim_Status = "Pending",
                        Handoff_Type = "Pickup",
                        Verification_Notes = "Accepted via chat notification"
                    };

             
                    var liveMsg = Messages.FirstOrDefault(m => m.LinkedItemId == msg.LinkedItemId
                                                            && m.IsSystemDirectTarget);
                    if (liveMsg != null) liveMsg.IsActionable = false;  // ← hides buttons immediately

                    var (success, error) = await KapwaDataService.SaveClaim(claim);
                    if (!success)
                    {
                        // If save failed, restore buttons
                        if (liveMsg != null) liveMsg.IsActionable = true;
                        MessageBox.Show(error, "Cannot Accept", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        await KapwaDataService.SaveChatMessage(_myId, _otherId,
                            $"✅ I have accepted the donation! Claim ID: {claimId}. " +
                            "Please confirm the handoff details.");
                        MessageBox.Show("✅ Donation accepted! Check your Claim Tracker.",
                            "Accepted", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadMessages();   // reload resets the collection — buttons stay hidden via DB state
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Accept failed: " + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally { IsBusy = false; }
            });

            DeclineCommand = new AsyncRelayCommand(async param =>
            {
                if (!IsBeneficiary) return;
                if (param is not ChatMessage msg) return;
                if (string.IsNullOrEmpty(msg.LinkedItemId)) return;

                // In DeclineCommand — after confirm == Yes:
                var liveMsg = Messages.FirstOrDefault(m => m.LinkedItemId == msg.LinkedItemId
                                                        && m.IsSystemDirectTarget);
                if (liveMsg != null) liveMsg.IsActionable = false;  // ← hides buttons immediately

                await KapwaDataService.RevertItemToGeneralPost(msg.LinkedItemId);
                await KapwaDataService.SaveChatMessage(_myId, _otherId,
                    "❌ I have declined the donation. The item has been returned to the marketplace.");
                MessageBox.Show("Item returned to marketplace.", "Declined",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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
                    ScrollToBottom?.Invoke();
                });
            }
            catch { }
        }
    }
}