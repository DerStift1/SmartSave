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

            if (_organizer is null)
            {
                _organizer = new DownloadOrganizerService();
                _organizer.Start();
            }

            if (_tray is null)
            {
                _tray = new TrayService();
                _tray.PendingRequested += OnPendingRequested;
                _tray.Start();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_tray is not null)
            {
                _tray.PendingRequested -= OnPendingRequested;
                _tray.Dispose();
                _tray = null;
            }

            _organizer?.Dispose();
            _organizer = null;
            base.OnExit(e);
        }

        private void OnPendingRequested(object? sender, System.EventArgs e)
        {
            _organizer?.ShowPending();
        }
    }
}
