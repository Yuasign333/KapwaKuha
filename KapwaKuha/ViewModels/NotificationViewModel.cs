using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class NotificationViewModel : ObservableObject
    {
        private readonly string _userId;

        // ── Properties ──────────────────────────────────────────────────────

        private int _unreadCount;
        public int UnreadCount
        {
            get => _unreadCount;
            set { _unreadCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasUnread)); }
        }

        public bool HasUnread => UnreadCount > 0;

        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set { _isPopupOpen = value; OnPropertyChanged(); }
        }

        public ObservableCollection<NotificationModel> NotificationsCollection { get; }
            = new ObservableCollection<NotificationModel>();

        // ── Commands ────────────────────────────────────────────────────────

        public ICommand LoadNotificationsCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }
        public ICommand TogglePopupCommand { get; }

        // ── Constructor ─────────────────────────────────────────────────────

        public NotificationViewModel(string userId)
        {
            _userId = userId;

            LoadNotificationsCommand = new AsyncRelayCommand(LoadNotificationsAsync);
            MarkAllAsReadCommand = new AsyncRelayCommand(MarkAllAsReadAsync);
            TogglePopupCommand = new RelayCommand(_ =>
            {
                IsPopupOpen = !IsPopupOpen;
                if (IsPopupOpen)
                    _ = LoadNotificationsAsync();
            });
        }

        // ── Data Methods ────────────────────────────────────────────────────

        public async Task LoadNotificationsAsync()
        {
            try
            {
                NotificationsCollection.Clear();

                using var conn = new SqlConnection(KapwaDataService.ConnectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_GetUserNotifications", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", _userId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    NotificationsCollection.Add(new NotificationModel
                    {
                        NotifId = reader["Notif_ID"].ToString(),
                        Title = reader["Title"].ToString(),
                        Message = reader["Message"].ToString(),
                        IsRead = Convert.ToBoolean(reader["IsRead"]),
                        SentAt = Convert.ToDateTime(reader["SentAt"]),
                        NotifType = reader["Notif_Type"].ToString(),
                        ReferenceId = reader["Reference_ID"].ToString()
                    });
                }

                await RefreshUnreadCountAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationViewModel] Load error: {ex.Message}");
            }
        }

        private async Task MarkAllAsReadAsync()
        {
            try
            {
                using var conn = new SqlConnection(KapwaDataService.ConnectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_MarkAllNotificationsRead", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", _userId);
                await cmd.ExecuteNonQueryAsync();

                // Update local collection
                foreach (var n in NotificationsCollection)
                    n.IsRead = true;

                UnreadCount = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationViewModel] MarkRead error: {ex.Message}");
            }
        }

        public async Task RefreshUnreadCountAsync()
        {
            try
            {
                using var conn = new SqlConnection(KapwaDataService.ConnectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_GetUnreadNotificationCount", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", _userId);

                var result = await cmd.ExecuteScalarAsync();
                UnreadCount = Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationViewModel] Count error: {ex.Message}");
            }
        }
    }
}