// FILE: Models/ChatMessage.cs
using KapwaKuha.ViewModels;

namespace KapwaKuha.Models
{
    public class ChatMessage : ObservableObject
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;

        // Linked item for DirectTarget system messages
        public string LinkedItemId { get; set; } = string.Empty;
        public string LinkedItemPath { get; set; } = string.Empty;

        // True = buttons show. False = user already responded, hide them immediately.
        private bool _isActionable = true;
        public bool IsActionable
        {
            get => _isActionable;
            set
            {
                _isActionable = value;
                OnPropertyChanged();
                // Recompute the compound visibility property
                OnPropertyChanged(nameof(IsAcceptableByReceiver));
            }
        }

        // True when: system DirectTarget notify AND current viewer is the receiver
        // AND they have not yet responded
        public bool IsAcceptableByReceiver =>
            IsSystemDirectTarget && !IsFromUser && IsActionable;

        // True when message is an auto-notify from the trigger
        public bool IsSystemDirectTarget =>
            !string.IsNullOrEmpty(LinkedItemId) && Text.Contains("reserved for you");

        private bool _isFromUser;
        public bool IsFromUser
        {
            get => _isFromUser;
            set
            {
                _isFromUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Alignment));
                OnPropertyChanged(nameof(BubbleBackground));
                OnPropertyChanged(nameof(IsAcceptableByReceiver));
            }
        }

        // Image message support
        public bool IsImageMessage => Text.StartsWith("[IMG]");
        public string ImagePath => IsImageMessage ? Text[5..] : string.Empty;

        public string Alignment => IsFromUser ? "Right" : "Left";
        public string BubbleBackground => IsFromUser ? "#00B4D8" : "#FFFFFF";
        public string BubbleTextColor => IsFromUser ? "#FFFFFF" : "#03045E";
    }
}