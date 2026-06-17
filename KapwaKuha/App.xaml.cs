using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Toolkit.Uwp.Notifications;

namespace KapwaKuha
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ToastNotificationManagerCompat.OnActivated += _ => { };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ToastNotificationManagerCompat.Uninstall();
            base.OnExit(e);
        }
    }
}