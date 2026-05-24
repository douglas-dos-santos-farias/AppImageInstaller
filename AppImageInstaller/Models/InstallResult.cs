namespace AppImageInstaller.Models;

public sealed record InstallResult(
    string InstalledAppImagePath,
    string InstalledIconPath,
    string DesktopEntryPath);
