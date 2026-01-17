using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace SmartSave.App.Services
{
    public sealed class TrayService : IDisposable
    {
        private TaskbarIcon? _trayIcon;

        public void Start()
        {
            _trayIcon = new TaskbarIcon
            {
                Icon = SystemIcons.Application,
                ToolTipText = "SmartSave"
            };

            var menu = new ContextMenu();

            var testItem = new MenuItem { Header = "Open (test)" };
            testItem.Click += (_, _) => MessageBox.Show("SmartSave läuft ✅", "SmartSave");
            menu.Items.Add(testItem);

            menu.Items.Add(new Separator());

            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += (_, _) => Application.Current.Shutdown();
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenu = menu;

            // Optional: Klick auf Tray-Icon macht auch "Open (test)"
            _trayIcon.TrayLeftMouseUp += (_, _) => MessageBox.Show("SmartSave läuft ✅", "SmartSave");
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
        }
    }
}
