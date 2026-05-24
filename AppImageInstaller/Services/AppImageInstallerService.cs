using System.Runtime.InteropServices;
using AppImageInstaller.Models;

namespace AppImageInstaller.Services;

public sealed class AppImageInstallerService(IDesktopEntryWriter desktopEntryWriter) : IAppImageInstallerService
{
    private const UnixFileMode ExecutableMode =
        UnixFileMode.UserRead |
        UnixFileMode.UserWrite |
        UnixFileMode.UserExecute |
        UnixFileMode.GroupRead |
        UnixFileMode.GroupExecute |
        UnixFileMode.OtherRead |
        UnixFileMode.OtherExecute;

    public async Task<InstallResult> InstallAsync(InstallRequest request, CancellationToken cancellationToken = default)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            throw new PlatformNotSupportedException("AppImage installation is supported only on Linux.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var baseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "share",
            "applications");

        Directory.CreateDirectory(baseDirectory);

        var slug = Slugify(request.DisplayName);
        var appImageExtension = Path.GetExtension(request.AppImageSourcePath);
        var iconExtension = Path.GetExtension(request.IconSourcePath);

        var installedAppImagePath = Path.Combine(baseDirectory, $"{slug}{appImageExtension}");
        var installedIconPath = Path.Combine(baseDirectory, $"{slug}{iconExtension}");
        var desktopEntryPath = Path.Combine(baseDirectory, $"{slug}.desktop");

        await CopyAsync(request.AppImageSourcePath, installedAppImagePath, cancellationToken);
        await CopyAsync(request.IconSourcePath, installedIconPath, cancellationToken);

        File.SetUnixFileMode(installedAppImagePath, ExecutableMode);

        await desktopEntryWriter.WriteAsync(
            desktopEntryPath,
            request.DisplayName,
            installedAppImagePath,
            installedIconPath,
            request.Category,
            cancellationToken);

        File.SetUnixFileMode(
            desktopEntryPath,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.GroupRead |
            UnixFileMode.OtherRead);

        return new InstallResult(installedAppImagePath, installedIconPath, desktopEntryPath);
    }

    private static async Task CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        await using var source = File.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using var destination = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await source.CopyToAsync(destination, cancellationToken);
    }

    private static string Slugify(string value)
    {
        var buffer = value
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        var slug = new string(buffer);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-') switch
        {
            "" => "appimage-app",
            var normalized => normalized
        };
    }
}
