using System.Text;

namespace AppImageInstaller.Services;

public sealed class DesktopEntryWriter : IDesktopEntryWriter
{
    public async Task<string> WriteAsync(
        string desktopEntryPath,
        string displayName,
        string execPath,
        string iconPath,
        string category,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var builder = new StringBuilder();
        builder.AppendLine("[Desktop Entry]");
        builder.AppendLine("Version=1.0");
        builder.AppendLine("Type=Application");
        builder.AppendLine($"Name={EscapeValue(displayName)}");
        builder.AppendLine($"Exec={EscapeValue(execPath)}");
        builder.AppendLine($"Icon={EscapeValue(iconPath)}");
        builder.AppendLine($"Categories={EscapeCategory(category)};");
        builder.AppendLine("Terminal=false");

        await File.WriteAllTextAsync(desktopEntryPath, builder.ToString(), cancellationToken);
        return desktopEntryPath;
    }

    private static string EscapeValue(string value) => value.Replace("\n", " ").Trim();

    private static string EscapeCategory(string category)
        => string.Concat(category.Where(char.IsLetterOrDigit));
}
