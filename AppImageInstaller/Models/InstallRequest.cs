namespace AppImageInstaller.Models;

public sealed record InstallRequest(
    string AppImageSourcePath,
    string IconSourcePath,
    string DisplayName,
    string Category);
