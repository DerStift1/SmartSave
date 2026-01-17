using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmartSave.App.Models;

namespace SmartSave.App.UI;

public partial class DestinationPopup : Window
{
    private readonly TaskCompletionSource<DestinationOption?> _selectionTcs = new();

    public DestinationPopup(string fileName, IReadOnlyList<DestinationOption> options)
    {
        InitializeComponent();
        FileNameText.Text = fileName;
        OptionsList.ItemsSource = options;
    }

    public Task<DestinationOption?> WaitForSelectionAsync() => _selectionTcs.Task;

    private void OptionClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DestinationOption option)
        {
            _selectionTcs.TrySetResult(option);
            Close();
        }
    }

    private void LaterClick(object sender, RoutedEventArgs e)
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
