namespace AppImageInstaller.Models;

public sealed record InstallRequest(
    string AppImageSourcePath,
    string IconSourcePath,
    string InstallDirectory,
    string DisplayName,
    string Category,
    IReadOnlyList<DesktopCustomField> CustomFields);
