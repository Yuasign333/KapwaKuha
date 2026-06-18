using System;
using KapwaKuha.ViewModels;

namespace KapwaKuha.Models
{
    public class NotificationModel : ObservableObject
    {
        public string Notif_ID { get; set; } = string.Empty;
        public string Recipient_ID { get; set; } = string.Empty;
        public string TargetRole { get; set; } = string.Empty;   // ADD THIS
        public string Title { get; set; } = string.Empty;         // ADD THIS
        public string Notif_Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        private bool _isRead;
        public bool IsRead
        {
            get => _isRead;
            set { _isRead = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsUnread)); }
        }

        public bool IsUnread => !_isRead;
        public DateTime SentAt { get; set; } = DateTime.Now;
        public string Reference_ID { get; set; } = string.Empty;
        public string SentAtDisplay => SentAt.ToString("MMM dd  HH:mm");

        public string TimeAgo
        {
            get
            {
                var diff = DateTime.Now - SentAt;
                if (diff.TotalMinutes < 1) return "Just now";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
                if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
                return SentAt.ToString("MMM dd");
            }
        }
        public string NotifIcon => Notif_Type switch
        {
            "ClaimUpdate" => "📦",
            "Approval" => "✅",
            "Message" => "💬",
            "AccountAlert" => "⚠️",
            _ => "🔔"
        };
    }
}