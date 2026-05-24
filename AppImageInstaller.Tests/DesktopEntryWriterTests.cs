using AppImageInstaller.Models;
using AppImageInstaller.Services;
using Xunit;

namespace AppImageInstaller.Tests;

public sealed class DesktopEntryWriterTests
{
    [Fact]
    public async Task WriteAsync_WritesStandardFieldsOnly_WhenNoCustomFields()
    {
        var writer = new DesktopEntryWriter();
        var path = Path.Combine(Path.GetTempPath(), $"appimage-installer-{Guid.NewGuid():N}.desktop");

        try
        {
            await writer.WriteAsync(path, "My App", "/tmp/app.AppImage", "/tmp/app.png", "Utility", []);

            var content = await File.ReadAllTextAsync(path);
            Assert.Contains("Name=My App", content);
            Assert.Contains("Exec=/tmp/app.AppImage", content);
            Assert.Contains("Icon=/tmp/app.png", content);
            Assert.Contains("Categories=Utility;", content);
            Assert.DoesNotContain("X-", content);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task WriteAsync_WritesCustomFields_WhenNoConflict()
    {
        var writer = new DesktopEntryWriter();
        var path = Path.Combine(Path.GetTempPath(), $"appimage-installer-{Guid.NewGuid():N}.desktop");

        try
        {
            await writer.WriteAsync(
                path,
                "My App",
                "/tmp/app.AppImage",
                "/tmp/app.png",
                "Utility",
                [new DesktopCustomField("X-Test", "hello")]);

            var content = await File.ReadAllTextAsync(path);
            Assert.Contains("X-Test=hello", content);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task WriteAsync_WritesCustomConflictAfterStandardFields()
    {
        var writer = new DesktopEntryWriter();
        var path = Path.Combine(Path.GetTempPath(), $"appimage-installer-{Guid.NewGuid():N}.desktop");

        try
        {
            await writer.WriteAsync(
                path,
                "My App",
                "/tmp/original.AppImage",
                "/tmp/app.png",
                "Utility",
                [new DesktopCustomField("Exec", "/tmp/override.AppImage")]);

            var content = await File.ReadAllTextAsync(path);
            var originalIndex = content.IndexOf("Exec=/tmp/original.AppImage", StringComparison.Ordinal);
            var overrideIndex = content.IndexOf("Exec=/tmp/override.AppImage", StringComparison.Ordinal);

            Assert.True(originalIndex >= 0);
            Assert.True(overrideIndex > originalIndex);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task WriteAsync_SanitizesLineBreaks_AndSkipsInvalidKeys()
    {
        var writer = new DesktopEntryWriter();
        var path = Path.Combine(Path.GetTempPath(), $"appimage-installer-{Guid.NewGuid():N}.desktop");

        try
        {
            await writer.WriteAsync(
                path,
                "My\nApp",
                "/tmp/app.AppImage",
                "/tmp/app.png",
                "Utility",
                [
                    new DesktopCustomField("X-Good", "line1\nline2"),
                    new DesktopCustomField("Bad Key", "value")
                ]);

            var content = await File.ReadAllTextAsync(path);
            Assert.Contains("Name=My App", content);
            Assert.Contains("X-Good=line1 line2", content);
            Assert.DoesNotContain("Bad Key=value", content);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
