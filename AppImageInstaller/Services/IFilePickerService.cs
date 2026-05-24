namespace AppImageInstaller.Services;

public interface IFilePickerService
{
    Task<string?> PickAppImageAsync(CancellationToken cancellationToken = default);
    Task<string?> PickIconAsync(CancellationToken cancellationToken = default);
}
