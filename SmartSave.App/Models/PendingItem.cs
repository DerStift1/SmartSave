using System.IO;

namespace SmartSave.App.Models;

public sealed class PendingItem
{
    public string FullPath { get; }
    public string DisplayName { get; }
    public string DetailsText { get; }

    public PendingItem(string fullPath)
    {
        FullPath = fullPath;
        var name = Path.GetFileName(fullPath);
        DisplayName = string.IsNullOrWhiteSpace(name) ? fullPath : name;
        DetailsText = Path.GetDirectoryName(fullPath) ?? string.Empty;
    }
}
