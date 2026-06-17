using Microsoft.Toolkit.Uwp.Notifications;

namespace KapwaKuha.Services
{
    public static class ToastService
    {
        public static void Show(string title, string message)
        {
            try
            {
                new ToastContentBuilder()
                    .AddAppLogoOverride(null)
                    .AddText(title)
                    .AddText(message)
                    .Show();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ToastService] Failed: {ex.Message}");
            }
        }
    }
}