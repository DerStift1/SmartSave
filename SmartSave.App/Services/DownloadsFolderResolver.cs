using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SmartSave.App.Services;

public static class DownloadsFolderResolver
{
    private static readonly Guid DownloadsFolderId = new("374DE290-123F-4565-9164-39C4925E467B");

    public static string GetDownloadsPath()
    {
        var path = TryGetKnownFolderPath(DownloadsFolderId);
        if (!string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(profile, "Downloads");
    }

    private static string? TryGetKnownFolderPath(Guid folderId)
    {
        var result = SHGetKnownFolderPath(folderId, 0, IntPtr.Zero, out var pathPtr);
        if (result != 0 || pathPtr == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            return Marshal.PtrToStringUni(pathPtr);
        }
        finally
        {
            Marshal.FreeCoTaskMem(pathPtr);
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHGetKnownFolderPath(Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
}
