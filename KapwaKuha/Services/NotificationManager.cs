using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using KapwaKuha.Services;

namespace KapwaKuha.Services
{
    /// <summary>
    /// Central coordinator for all three notification channels:
    /// 1. In-app (SQL insert)
    /// 2. Windows Toast (OS-level popup)
    /// 3. External: SMS via Semaphore OR Email via SMTP (based on user preference)
    /// </summary>
    public static class NotificationManager
    {
        public static async Task TriggerNotificationAsync(
            string userId,
            string role,
            string title,
            string message,
            string email = "",
            string phone = "",
            string preference = "Email",
            string notifType = "AccountAlert",
            string referenceId = "")
        {
            // ── Step 1: Write to DB (In-App bell log) ──────────────────────
            await SaveToDbAsync(userId, role, title, message, notifType, referenceId);

            // ── Step 2: OS Toast notification ──────────────────────────────
            // Must run on UI thread; dispatcher-safe via try-catch
            try
            {
                ToastService.Show(title, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationManager] Toast error: {ex.Message}");
            }

            // ── Step 3: External channel based on preference ────────────────
            preference = preference?.Trim().ToLower() ?? "email";

            switch (preference)
            {
                case "sms":
                    if (!string.IsNullOrWhiteSpace(phone))
                        await SmsService.SendAsync(phone, $"{title}: {message}");
                    break;

                case "both":
                    if (!string.IsNullOrWhiteSpace(email))
                        await EmailService.SendAsync(email, $"KapwaKuha — {title}", message);
                    if (!string.IsNullOrWhiteSpace(phone))
                        await SmsService.SendAsync(phone, $"{title}: {message}");
                    break;

                case "email":
                default:
                    if (!string.IsNullOrWhiteSpace(email))
                        await EmailService.SendAsync(email, $"KapwaKuha — {title}", message);
                    break;
            }
        }

        private static async Task SaveToDbAsync(
            string userId,
            string role,
            string title,
            string message,
            string notifType,
            string referenceId)
        {
            try
            {
                using var conn = new SqlConnection(KapwaDataService.ConnectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_InsertNotification", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RecipientId", userId);
                cmd.Parameters.AddWithValue("@TargetRole", role);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@NotifType", notifType);
                cmd.Parameters.AddWithValue("@ReferenceId", referenceId ?? "");

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationManager] DB error: {ex.Message}");
            }
        }
    }
}