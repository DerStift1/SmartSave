using System.IO;

namespace SmartSave.App.Models;

public enum DestinationKind
{
    ExistingFolder,
    CreateFolder
}

public sealed class DestinationOption
{
    public DestinationKind Kind { get; }
    public string DisplayName { get; }
    public string DetailsText { get; }
    public string TargetPath { get; }

    private DestinationOption(DestinationKind kind, string displayName, string detailsText, string targetPath)
    {
        Kind = kind;
        DisplayName = displayName;
        DetailsText = detailsText;
        TargetPath = targetPath;
    }

    public static DestinationOption ExistingFolder(string folderPath)
    {
        var name = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(name))
        {
            name = folderPath;
        }

        return new DestinationOption(
            DestinationKind.ExistingFolder,
            name,
            folderPath,
            folderPath);
    }

    public static DestinationOption CreateFolder(string parentFolder)
    {
        return new DestinationOption(
            DestinationKind.CreateFolder,
            "Create folder + Move",
            parentFolder,
            parentFolder);
    }
}
