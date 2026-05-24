using System.Text;
using AppImageInstaller.Models;

namespace AppImageInstaller.Services;

public sealed class DesktopEntryWriter : IDesktopEntryWriter
{
    public async Task<string> WriteAsync(
        string desktopEntryPath,
        string displayName,
        string execPath,
        string iconPath,
        string category,
        IReadOnlyList<DesktopCustomField> customFields,
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

        foreach (var field in customFields)
        {
            var key = SanitizeKey(field.Key);
            var value = SanitizeValue(field.Value);
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            builder.AppendLine($"{key}={value}");
        }

        await File.WriteAllTextAsync(desktopEntryPath, builder.ToString(), cancellationToken);
        return desktopEntryPath;
    }

    private static string EscapeValue(string value) => value.Replace("\n", " ").Trim();

    private static string EscapeCategory(string category)
        => string.Concat(category.Where(char.IsLetterOrDigit));

    private static string SanitizeKey(string key)
    {
        var sanitized = key.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        return sanitized.Any(char.IsWhiteSpace) ? string.Empty : sanitized;
    }

    private static string SanitizeValue(string value)
        => value.Replace("\r", " ").Replace("\n", " ").Trim();
}
