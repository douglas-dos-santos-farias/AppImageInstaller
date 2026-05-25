namespace AppImageInstaller.Services;

public interface IAppSettingsService
{
    string? LoadLastInstallDirectory();
    string? LoadThemeKey();
    Task SaveLastInstallDirectoryAsync(string path, CancellationToken cancellationToken = default);
    Task SaveThemeKeyAsync(string themeKey, CancellationToken cancellationToken = default);
}
