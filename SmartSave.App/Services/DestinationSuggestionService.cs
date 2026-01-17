using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartSave.App.Services;

public sealed class DestinationSuggestionService
{
    public IReadOnlyList<string> GetTopFolders(string downloadsPath, int count)
    {
        if (!Directory.Exists(downloadsPath))
        {
            return [];
        }

        return Directory.GetDirectories(downloadsPath)
            .Select(path => new DirectoryInfo(path))
            .Where(info => (info.Attributes & FileAttributes.Hidden) == 0)
            .OrderByDescending(info => info.LastWriteTimeUtc)
            .Take(count)
            .Select(info => info.FullName)
            .ToList();
    }
}
