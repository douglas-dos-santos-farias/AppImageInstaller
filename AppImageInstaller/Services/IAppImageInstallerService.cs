using AppImageInstaller.Models;

namespace AppImageInstaller.Services;

public interface IAppImageInstallerService
{
    Task<InstallResult> InstallAsync(InstallRequest request, CancellationToken cancellationToken = default);
}
