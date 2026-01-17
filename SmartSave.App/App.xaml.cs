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
            //MessageBox.Show("OnStartup reached ✅", "SmartSave");
            base.OnStartup(e);

            _tray = new TrayService();
            _tray.Start();

            _organizer = new DownloadOrganizerService();
            _organizer.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _organizer?.Dispose();
            _tray?.Dispose();
            base.OnExit(e);
        }
    }
}


