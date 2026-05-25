using System.Text.Json;

namespace AppImageInstaller.Services;

public sealed class AppSettingsService : IAppSettingsService
{
    private readonly string settingsPath;

    public AppSettingsService()
    {
        var configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "AppImageInstaller");

        settingsPath = Path.Combine(configDirectory, "settings.json");
    }

    public string? LoadLastInstallDirectory()
    {
        var data = LoadSettings();
        return string.IsNullOrWhiteSpace(data?.LastInstallDirectory) ? null : data.LastInstallDirectory;
    }

    public string? LoadThemeKey()
    {
        var data = LoadSettings();
        return string.IsNullOrWhiteSpace(data?.ThemeKey) ? null : data.ThemeKey;
    }

    public async Task SaveLastInstallDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var data = LoadSettings() ?? new AppSettingsData();
        data.LastInstallDirectory = path;
        await SaveSettingsAsync(data, cancellationToken);
    }

    public async Task SaveThemeKeyAsync(string themeKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(themeKey))
        {
            return;
        }

        var data = LoadSettings() ?? new AppSettingsData();
        data.ThemeKey = themeKey;
        await SaveSettingsAsync(data, cancellationToken);
    }

    private AppSettingsData? LoadSettings()
    {
        if (!File.Exists(settingsPath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(settingsPath);
            return JsonSerializer.Deserialize<AppSettingsData>(json);
        }
        catch
        {
            return null;
        }
    }

    private async Task SaveSettingsAsync(AppSettingsData data, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(settingsPath)!;
        Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(settingsPath, json, cancellationToken);
    }

    private sealed class AppSettingsData
    {
        public string? LastInstallDirectory { get; set; }
        public string? ThemeKey { get; set; }
    }
}
