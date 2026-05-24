namespace AppImageInstaller.Services;

public interface IAppSettingsService
{
    string? LoadLastInstallDirectory();
    Task SaveLastInstallDirectoryAsync(string path, CancellationToken cancellationToken = default);
}
