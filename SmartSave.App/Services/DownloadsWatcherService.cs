using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartSave.App.Models;

namespace SmartSave.App.Services;

public sealed class DownloadsWatcherService : IDisposable
{
    private static readonly string[] TemporaryExtensions = [".crdownload", ".tmp"];

    private readonly FileSystemWatcher _watcher;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _pending = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _stableDuration = TimeSpan.FromMilliseconds(1500);
    private readonly TimeSpan _pollInterval = TimeSpan.FromMilliseconds(500);

    public string DownloadsPath { get; }

    public event EventHandler<FileReadyEventArgs>? FileReady;

    public DownloadsWatcherService(string? downloadsPath = null)
    {
        DownloadsPath = string.IsNullOrWhiteSpace(downloadsPath)
            ? DownloadsFolderResolver.GetDownloadsPath()
            : downloadsPath;

        Directory.CreateDirectory(DownloadsPath);
        _watcher = new FileSystemWatcher(DownloadsPath)
        {
            IncludeSubdirectories = false,
            Filter = "*",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Renamed += OnRenamed;
    }

    public void Start()
    {
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
    }

    public void Dispose()
    {
        Stop();
        _watcher.Created -= OnCreated;
        _watcher.Changed -= OnChanged;
        _watcher.Renamed -= OnRenamed;
        _watcher.Dispose();

        foreach (var entry in _pending)
        {
            entry.Value.Cancel();
            entry.Value.Dispose();
        }

        _pending.Clear();
    }

    private void OnCreated(object sender, FileSystemEventArgs e) => QueueCandidate(e.FullPath);

    private void OnChanged(object sender, FileSystemEventArgs e) => QueueCandidate(e.FullPath);

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        CancelPending(e.OldFullPath);
        QueueCandidate(e.FullPath);
    }

    private void QueueCandidate(string fullPath)
    {
        if (Directory.Exists(fullPath))
        {
            return;
        }

        if (IsTemporaryExtension(fullPath))
        {
            return;
        }

        var cts = new CancellationTokenSource();
        var existing = _pending.GetOrAdd(fullPath, cts);
        if (existing != cts)
        {
            cts.Dispose();
            return;
        }

        _ = WaitForReadyAsync(fullPath, cts);
    }

    private void CancelPending(string fullPath)
    {
        if (_pending.TryRemove(fullPath, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    private async Task WaitForReadyAsync(string fullPath, CancellationTokenSource cts)
    {
        try
        {
            long lastSize = -1;
            var lastWriteTime = DateTime.MinValue;
            var stableSince = DateTime.UtcNow;

            while (!cts.IsCancellationRequested)
            {
                if (!File.Exists(fullPath))
                {
                    return;
                }

                if (IsTemporaryExtension(fullPath))
                {
                    return;
                }

                var info = new FileInfo(fullPath);
                if (info.Length != lastSize || info.LastWriteTimeUtc != lastWriteTime)
                {
                    lastSize = info.Length;
                    lastWriteTime = info.LastWriteTimeUtc;
                    stableSince = DateTime.UtcNow;
                }

                var stableEnough = DateTime.UtcNow - stableSince >= _stableDuration;
                if (stableEnough && CanOpenExclusive(fullPath))
                {
                    FileReady?.Invoke(this, new FileReadyEventArgs(fullPath));
                    return;
                }

                await Task.Delay(_pollInterval, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellations.
        }
        finally
        {
            CancelPending(fullPath);
        }
    }

    private static bool IsTemporaryExtension(string fullPath)
    {
        var extension = Path.GetExtension(fullPath);
        foreach (var temp in TemporaryExtensions)
        {
            if (string.Equals(extension, temp, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanOpenExclusive(string fullPath)
    {
        try
        {
            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.None);
            return stream.Length >= 0;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
