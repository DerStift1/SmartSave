using System.Windows;
using SmartSave.App.Services;

namespace SmartSave.App
{
    public partial class App : Application
    {
        private TrayService? _tray;
        private DownloadOrganizerService? _organizer;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _organizer = new DownloadOrganizerService();
            _organizer.Start();

            _tray = new TrayService();
            _tray.PendingRequested += OnPendingRequested;
            _tray.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_tray is not null)
            {
                _tray.PendingRequested -= OnPendingRequested;
            }

            _organizer?.Dispose();
            _tray?.Dispose();
            base.OnExit(e);
        }

        private void OnPendingRequested(object? sender, System.EventArgs e)
        {
            _organizer?.ShowPending();
        }
    }
}
