using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AppImageInstaller.Services;

public sealed class AvaloniaFilePickerService(TopLevel topLevel) : IFilePickerService
{
    public async Task<string?> PickAppImageAsync(CancellationToken cancellationToken = default)
    {
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select an AppImage",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("AppImage")
                {
                    Patterns = ["*.AppImage", "*.appimage"],
                    MimeTypes = ["application/octet-stream"]
                }
            ]
        });

        return files.FirstOrDefault()?.TryGetLocalPath();
    }

    public async Task<string?> PickIconAsync(CancellationToken cancellationToken = default)
    {
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select an icon",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Images")
                {
                    Patterns = ["*.png", "*.svg", "*.jpg", "*.jpeg", "*.webp"]
                }
            ]
        });

        return files.FirstOrDefault()?.TryGetLocalPath();
    }
}
