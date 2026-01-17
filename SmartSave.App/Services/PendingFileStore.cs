using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartSave.App.Services;

public sealed class PendingFileStore
{
    private readonly HashSet<string> _pending = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public bool Add(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return false;
        }

        lock (_lock)
        {
            return _pending.Add(fullPath);
        }
    }

    public bool Remove(string fullPath)
    {
        lock (_lock)
        {
            return _pending.Remove(fullPath);
        }
    }

    public bool Contains(string fullPath)
    {
        lock (_lock)
        {
            return _pending.Contains(fullPath);
        }
    }

    public IReadOnlyList<string> GetExisting()
    {
        List<string> snapshot;
        lock (_lock)
        {
            snapshot = _pending.ToList();
        }

        var existing = snapshot.Where(File.Exists).ToList();
        if (existing.Count == snapshot.Count)
        {
            return existing;
        }

        lock (_lock)
        {
            _pending.Clear();
            foreach (var path in existing)
            {
                _pending.Add(path);
            }
        }

        return existing;
    }
}
