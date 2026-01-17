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

        public event EventHandler? PendingRequested;

        public void Start()
        {
            _trayIcon = new TaskbarIcon
            {
                Icon = SystemIcons.Application,
                ToolTipText = "SmartSave"
            };

            var menu = new ContextMenu();

            var pendingItem = new MenuItem { Header = "Pending..." };
            pendingItem.Click += (_, _) => PendingRequested?.Invoke(this, EventArgs.Empty);
            menu.Items.Add(pendingItem);

            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += (_, _) => Application.Current.Shutdown();
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenu = menu;
            _trayIcon.TrayLeftMouseUp += (_, _) => PendingRequested?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
        }
    }
}
