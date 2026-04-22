// FILE: ChatMessage.cs
// DB Table: ChatMessages
// Parallel to ChatMessage in CarRentals — bubble alignment via IsFromUser
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
            }
        }

        // "Right" if sent by current user, "Left" if received
        public string Alignment => IsFromUser ? "Right" : "Left";

        // Teal bubble for sent, white for received
        public string BubbleBackground => IsFromUser ? "#00B4D8" : "#FFFFFF";
        public string BubbleTextColor => IsFromUser ? "#FFFFFF" : "#03045E";
    }
}