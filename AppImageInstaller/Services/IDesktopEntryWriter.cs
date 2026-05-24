using AppImageInstaller.Models;

namespace AppImageInstaller.Services;

public interface IDesktopEntryWriter
{
    Task<string> WriteAsync(
        string desktopEntryPath,
        string displayName,
        string execPath,
        string iconPath,
        string category,
        IReadOnlyList<DesktopCustomField> customFields,
        CancellationToken cancellationToken = default);
}
