using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SmartSave.App.Models;
using SmartSave.App.UI;

namespace SmartSave.App.Services;

public sealed class DownloadOrganizerService : IDisposable
{
    private readonly DownloadsWatcherService _watcher;
    private readonly DestinationSuggestionService _suggestions = new();
    private readonly MoveService _moveService = new();
    private readonly PendingFileStore _pendingStore = new();
    private readonly SemaphoreSlim _promptGate = new(1, 1);

    public DownloadOrganizerService()
    {
        _watcher = new DownloadsWatcherService();
        _watcher.FileReady += OnFileReady;
    }

    public void Start() => _watcher.Start();

    public void ShowPending() => _ = HandlePendingAsync();

    public void Dispose()
    {
        _watcher.FileReady -= OnFileReady;
        _watcher.Dispose();
        _promptGate.Dispose();
    }

    private void OnFileReady(object? sender, FileReadyEventArgs e)
    {
        _ = HandleFileReadyAsync(e.FullPath);
    }

    private async Task HandleFileReadyAsync(string fullPath)
    {
        await _promptGate.WaitAsync();
        try
        {
            if (_pendingStore.Contains(fullPath))
            {
                return;
            }

            if (!File.Exists(fullPath))
            {
                return;
            }

            var options = BuildOptions(_watcher.DownloadsPath);
            var selection = await ShowPopupAsync(fullPath, options);
            if (selection is null)
            {
                _pendingStore.Add(fullPath);
                return;
            }

            MoveFile(fullPath, selection);
            _pendingStore.Remove(fullPath);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            _promptGate.Release();
        }
    }

    private async Task HandlePendingAsync()
    {
        await _promptGate.WaitAsync();
        try
        {
            var pending = _pendingStore.GetExisting();
            if (pending.Count == 0)
            {
                await ShowInfoAsync("No pending downloads.");
                return;
            }

            var selected = await ShowPendingPopupAsync(pending);
            if (string.IsNullOrWhiteSpace(selected))
            {
                return;
            }

            if (!File.Exists(selected))
            {
                _pendingStore.Remove(selected);
                await ShowInfoAsync("That file is no longer available.");
                return;
            }

            var options = BuildOptions(_watcher.DownloadsPath);
            var selection = await ShowPopupAsync(selected, options);
            if (selection is null)
            {
                _pendingStore.Add(selected);
                return;
            }

            MoveFile(selected, selection);
            _pendingStore.Remove(selected);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            _promptGate.Release();
        }
    }

    private void MoveFile(string fullPath, DestinationOption selection)
    {
        if (selection.Kind == DestinationKind.ExistingFolder)
        {
            _moveService.MoveToFolder(fullPath, selection.TargetPath);
        }
        else
        {
            _moveService.MoveToNewFolder(fullPath, selection.TargetPath);
        }
    }

    private List<DestinationOption> BuildOptions(string downloadsPath)
    {
        var options = new List<DestinationOption>();
        foreach (var folder in _suggestions.GetTopFolders(downloadsPath, 3))
        {
            options.Add(DestinationOption.ExistingFolder(folder));
        }

        options.Add(DestinationOption.CreateFolder(downloadsPath));
        return options;
    }

    private Task<DestinationOption?> ShowPopupAsync(string fullPath, IReadOnlyList<DestinationOption> options)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            return ShowPopupInternal(fullPath, options);
        }

        return dispatcher.InvokeAsync(() => ShowPopupInternal(fullPath, options)).Task.Unwrap();
    }

    private static Task<DestinationOption?> ShowPopupInternal(string fullPath, IReadOnlyList<DestinationOption> options)
    {
        var popup = new DestinationPopup(Path.GetFileName(fullPath), options);
        popup.Show();
        return popup.WaitForSelectionAsync();
    }

    private Task<string?> ShowPendingPopupAsync(IReadOnlyList<string> pending)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            return ShowPendingPopupInternal(pending);
        }

        return dispatcher.InvokeAsync(() => ShowPendingPopupInternal(pending)).Task.Unwrap();
    }

    private static Task<string?> ShowPendingPopupInternal(IReadOnlyList<string> pending)
    {
        var popup = new PendingPopup(pending);
        popup.Show();
        return popup.WaitForSelectionAsync();
    }

    private static Task ShowErrorAsync(Exception ex)
    {
        return ShowMessageAsync(ex.ToString(), "SmartSave");
    }

    private static Task ShowInfoAsync(string message)
    {
        return ShowMessageAsync(message, "SmartSave");
    }

    private static Task ShowMessageAsync(string message, string caption)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            return Task.CompletedTask;
        }

        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(message, caption);
            return Task.CompletedTask;
        }

        return dispatcher.InvokeAsync(() => MessageBox.Show(message, caption)).Task;
    }
}
