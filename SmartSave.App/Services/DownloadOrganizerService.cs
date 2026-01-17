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
    private readonly SemaphoreSlim _promptGate = new(1, 1);

    public DownloadOrganizerService()
    {
        _watcher = new DownloadsWatcherService();
        _watcher.FileReady += OnFileReady;
    }

    public void Start() => _watcher.Start();

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
            if (!File.Exists(fullPath))
            {
                return;
            }

            var options = BuildOptions(_watcher.DownloadsPath);
            var selection = await ShowPopupAsync(fullPath, options);
            if (selection is null)
            {
                return;
            }

            if (selection.Kind == DestinationKind.ExistingFolder)
            {
                _moveService.MoveToFolder(fullPath, selection.TargetPath);
            }
            else
            {
                _moveService.MoveToNewFolder(fullPath, selection.TargetPath);
            }
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

    private static Task ShowErrorAsync(Exception ex)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(ex.Message, "SmartSave");
            return Task.CompletedTask;
        }

        return dispatcher.InvokeAsync(() => MessageBox.Show(ex.Message, "SmartSave")).Task;
    }
}
