using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmartSave.App.Models;

namespace SmartSave.App.UI;

public partial class PendingPopup : Window
{
    private readonly TaskCompletionSource<string?> _selectionTcs = new();

    public PendingPopup(IReadOnlyList<string> pendingPaths)
    {
        InitializeComponent();
        PendingList.ItemsSource = pendingPaths.Select(path => new PendingItem(path)).ToList();
    }

    public Task<string?> WaitForSelectionAsync() => _selectionTcs.Task;

    private void PendingClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is PendingItem item)
        {
            _selectionTcs.TrySetResult(item.FullPath);
            Close();
        }
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        _selectionTcs.TrySetResult(null);
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _selectionTcs.TrySetResult(null);
        base.OnClosed(e);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - 12;
        Top = workArea.Bottom - ActualHeight - 12;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _selectionTcs.TrySetResult(null);
            Close();
        }
    }
}
