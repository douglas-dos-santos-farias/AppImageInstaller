namespace AppImageInstaller.Services;

public interface IDesktopEntryWriter
{
    Task<string> WriteAsync(
        string desktopEntryPath,
        string displayName,
        string execPath,
        string iconPath,
        string category,
        CancellationToken cancellationToken = default);
}
