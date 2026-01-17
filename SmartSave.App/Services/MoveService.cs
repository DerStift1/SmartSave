using System;
using System.IO;
using System.Text;

namespace SmartSave.App.Services;

public sealed class MoveService
{
    public string MoveToFolder(string sourcePath, string destinationFolder)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Source file not found.", sourcePath);
        }

        Directory.CreateDirectory(destinationFolder);
        var destinationPath = GetAvailableFilePath(destinationFolder, Path.GetFileName(sourcePath));
        File.Move(sourcePath, destinationPath);
        return destinationPath;
    }

    public string MoveToNewFolder(string sourcePath, string parentFolder)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Source file not found.", sourcePath);
        }

        var baseName = Path.GetFileNameWithoutExtension(sourcePath);
        var folderName = SanitizeFolderName(baseName);
        var destinationFolder = GetAvailableFolderPath(parentFolder, folderName);
        Directory.CreateDirectory(destinationFolder);
        var destinationPath = GetAvailableFilePath(destinationFolder, Path.GetFileName(sourcePath));
        File.Move(sourcePath, destinationPath);
        return destinationPath;
    }

    private static string GetAvailableFilePath(string folderPath, string fileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var candidate = Path.Combine(folderPath, fileName);
        var index = 1;

        while (File.Exists(candidate))
        {
            candidate = Path.Combine(folderPath, $"{baseName} ({index}){extension}");
            index++;
        }

        return candidate;
    }

    private static string GetAvailableFolderPath(string parentFolder, string folderName)
    {
        var candidate = Path.Combine(parentFolder, folderName);
        var index = 1;

        while (Directory.Exists(candidate))
        {
            candidate = Path.Combine(parentFolder, $"{folderName} ({index})");
            index++;
        }

        return candidate;
    }

    private static string SanitizeFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "New Folder";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(name.Length);
        foreach (var ch in name)
        {
            var isInvalid = Array.IndexOf(invalidChars, ch) >= 0;
            builder.Append(isInvalid ? '_' : ch);
        }

        var sanitized = builder.ToString().Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "New Folder" : sanitized;
    }
}
