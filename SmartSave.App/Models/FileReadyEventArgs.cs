using System;

namespace SmartSave.App.Models;

public sealed class FileReadyEventArgs : EventArgs
{
    public string FullPath { get; }

    public FileReadyEventArgs(string fullPath)
    {
        FullPath = fullPath;
    }
}
