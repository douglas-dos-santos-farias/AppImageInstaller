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
        if (!File.Exists(settingsPath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(settingsPath);
            var data = JsonSerializer.Deserialize<AppSettingsData>(json);
            return string.IsNullOrWhiteSpace(data?.LastInstallDirectory) ? null : data.LastInstallDirectory;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveLastInstallDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var directory = Path.GetDirectoryName(settingsPath)!;
        Directory.CreateDirectory(directory);

        var data = new AppSettingsData { LastInstallDirectory = path };
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(settingsPath, json, cancellationToken);
    }

    private sealed class AppSettingsData
    {
        public string? LastInstallDirectory { get; set; }
    }
}
