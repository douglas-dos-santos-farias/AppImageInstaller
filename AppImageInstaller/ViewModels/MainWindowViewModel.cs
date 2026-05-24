using System.Collections.ObjectModel;
using System.Windows.Input;
using AppImageInstaller.Models;
using AppImageInstaller.Services;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AppImageInstaller.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IFilePickerService filePickerService;
    private readonly IAppImageInstallerService installerService;
    private readonly IAppSettingsService appSettingsService;
    private readonly Action<string> applyTheme;
    private Bitmap? iconPreview;
    private string? selectedAppImagePath;
    private string? selectedIconPath;
    private string displayName = string.Empty;
    private string selectedCategory = "Utility";
    private string selectedThemeKey = "Light";
    private string appImageError = string.Empty;
    private string iconError = string.Empty;
    private string displayNameError = string.Empty;
    private string categoryError = string.Empty;
    private string installLocationError = string.Empty;
    private string customFieldError = string.Empty;
    private string? standardExecOverride;
    private string? standardIconOverride;
    private string standardTerminalOverride = "false";
    private string standardTypeOverride = "Application";
    private string standardVersionOverride = "1.0";
    private string installLocation = DefaultInstallLocation;
    private bool isResultModalVisible;
    private bool isAdvancedSettingsModalVisible;
    private string resultModalTitle = string.Empty;
    private string resultModalMessage = string.Empty;
    private bool resultIsSuccess;
    private bool isInstalling;

    public MainWindowViewModel(
        IFilePickerService filePickerService,
        IAppImageInstallerService installerService,
        IAppSettingsService appSettingsService,
        Action<string> applyTheme)
    {
        this.filePickerService = filePickerService;
        this.installerService = installerService;
        this.appSettingsService = appSettingsService;
        this.applyTheme = applyTheme;

        Categories = new ObservableCollection<string>
        {
            "AudioVideo",
            "Development",
            "Education",
            "Game",
            "Graphics",
            "Network",
            "Office",
            "Settings",
            "System",
            "Utility"
        };
        CustomFields = new ObservableCollection<DesktopCustomFieldInput>();

        InitializeInstallLocation();

        PickAppImageCommand = new AsyncCommand(PickAppImageAsync, () => !IsInstalling);
        PickIconCommand = new AsyncCommand(PickIconAsync, () => !IsInstalling);
        PickInstallDirectoryCommand = new AsyncCommand(PickInstallDirectoryAsync, () => !IsInstalling);
        InstallCommand = new AsyncCommand(InstallAsync, () => !IsInstalling);
        ResetInstallDirectoryCommand = new RelayCommand(ResetInstallDirectory, () => !IsInstalling);
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        CloseResultModalCommand = new RelayCommand(CloseResultModal);
        OpenAdvancedSettingsModalCommand = new RelayCommand(OpenAdvancedSettingsModal, () => !IsInstalling);
        CloseAdvancedSettingsModalCommand = new RelayCommand(CloseAdvancedSettingsModal, () => !IsInstalling);
        AddCustomFieldCommand = new RelayCommand(AddCustomField, () => !IsInstalling);
        RemoveCustomFieldCommand = new RelayCommand(RemoveCustomField, () => !IsInstalling);
        applyTheme(selectedThemeKey);
    }

    private static string DefaultInstallLocation => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".local",
        "share",
        "applications");

    public ObservableCollection<string> Categories { get; }
    public ICommand PickAppImageCommand { get; }
    public ICommand PickIconCommand { get; }
    public ICommand PickInstallDirectoryCommand { get; }
    public ICommand InstallCommand { get; }
    public ICommand ResetInstallDirectoryCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand CloseResultModalCommand { get; }
    public ICommand OpenAdvancedSettingsModalCommand { get; }
    public ICommand CloseAdvancedSettingsModalCommand { get; }
    public ICommand AddCustomFieldCommand { get; }
    public ICommand RemoveCustomFieldCommand { get; }

    public string? SelectedAppImagePath
    {
        get => selectedAppImagePath;
        private set
        {
            if (SetProperty(ref selectedAppImagePath, value))
            {
                RaisePropertyChanged(nameof(AppImageFileName));
                RaisePropertyChanged(nameof(StandardExecOverride));
                RaisePropertyChanged(nameof(CanInstall));
            }
        }
    }

    public string? SelectedIconPath
    {
        get => selectedIconPath;
        private set
        {
            if (SetProperty(ref selectedIconPath, value))
            {
                RaisePropertyChanged(nameof(IconFileName));
                RaisePropertyChanged(nameof(StandardIconOverride));
                RaisePropertyChanged(nameof(CanInstall));
            }
        }
    }

    public string DisplayName
    {
        get => displayName;
        set
        {
            if (SetProperty(ref displayName, value))
            {
                RaisePropertyChanged(nameof(StandardNameOverride));
                RaisePropertyChanged(nameof(CanInstall));
            }
        }
    }

    public string SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (SetProperty(ref selectedCategory, value))
            {
                RaisePropertyChanged(nameof(StandardCategoriesOverride));
                RaisePropertyChanged(nameof(CanInstall));
            }
        }
    }

    public string InstallLocation
    {
        get => installLocation;
        private set
        {
            if (SetProperty(ref installLocation, value))
            {
                RaisePropertyChanged(nameof(CanInstall));
            }
        }
    }

    public ObservableCollection<DesktopCustomFieldInput> CustomFields { get; }

    public string ThemeGlyph => selectedThemeKey == "Dark" ? "☾" : "☀";

    public string ThemeLabel => selectedThemeKey switch
    {
        "Light" => "Light mode",
        "Dark" => "Dark mode",
        _ => "Light mode"
    };

    public string StandardNameOverride
    {
        get => DisplayName;
        set => DisplayName = value;
    }

    public string StandardExecOverride
    {
        get => string.IsNullOrWhiteSpace(standardExecOverride) ? (SelectedAppImagePath ?? string.Empty) : standardExecOverride;
        set => SetProperty(ref standardExecOverride, value);
    }

    public string StandardIconOverride
    {
        get => string.IsNullOrWhiteSpace(standardIconOverride) ? (SelectedIconPath ?? string.Empty) : standardIconOverride;
        set => SetProperty(ref standardIconOverride, value);
    }

    public string StandardCategoriesOverride
    {
        get => SelectedCategory;
        set => SelectedCategory = value;
    }

    public string StandardTerminalOverride
    {
        get => standardTerminalOverride;
        set => SetProperty(ref standardTerminalOverride, value);
    }

    public string StandardTypeOverride
    {
        get => standardTypeOverride;
        set => SetProperty(ref standardTypeOverride, value);
    }

    public string StandardVersionOverride
    {
        get => standardVersionOverride;
        set => SetProperty(ref standardVersionOverride, value);
    }

    public string AppImageError
    {
        get => appImageError;
        private set
        {
            if (SetProperty(ref appImageError, value))
            {
                RaisePropertyChanged(nameof(HasAppImageError));
            }
        }
    }

    public string IconError
    {
        get => iconError;
        private set
        {
            if (SetProperty(ref iconError, value))
            {
                RaisePropertyChanged(nameof(HasIconError));
            }
        }
    }

    public string DisplayNameError
    {
        get => displayNameError;
        private set
        {
            if (SetProperty(ref displayNameError, value))
            {
                RaisePropertyChanged(nameof(HasDisplayNameError));
            }
        }
    }

    public string CategoryError
    {
        get => categoryError;
        private set
        {
            if (SetProperty(ref categoryError, value))
            {
                RaisePropertyChanged(nameof(HasCategoryError));
            }
        }
    }

    public string InstallLocationError
    {
        get => installLocationError;
        private set
        {
            if (SetProperty(ref installLocationError, value))
            {
                RaisePropertyChanged(nameof(HasInstallLocationError));
            }
        }
    }

    public string CustomFieldError
    {
        get => customFieldError;
        private set
        {
            if (SetProperty(ref customFieldError, value))
            {
                RaisePropertyChanged(nameof(HasCustomFieldError));
            }
        }
    }

    public bool IsInstalling
    {
        get => isInstalling;
        private set
        {
            if (SetProperty(ref isInstalling, value))
            {
                RaisePropertyChanged(nameof(CanInstall));
                RaiseCommands();
            }
        }
    }

    public bool CanInstall => !IsInstalling &&
                              !string.IsNullOrWhiteSpace(SelectedAppImagePath) &&
                              !string.IsNullOrWhiteSpace(SelectedIconPath) &&
                              !string.IsNullOrWhiteSpace(DisplayName) &&
                              !string.IsNullOrWhiteSpace(SelectedCategory) &&
                              !string.IsNullOrWhiteSpace(InstallLocation);

    public string AppImageFileName => Path.GetFileName(SelectedAppImagePath) ?? "No AppImage selected";
    public string IconFileName => Path.GetFileName(SelectedIconPath) ?? "No icon selected";
    public bool HasAppImageError => !string.IsNullOrWhiteSpace(AppImageError);
    public bool HasIconError => !string.IsNullOrWhiteSpace(IconError);
    public bool HasDisplayNameError => !string.IsNullOrWhiteSpace(DisplayNameError);
    public bool HasCategoryError => !string.IsNullOrWhiteSpace(CategoryError);
    public bool HasInstallLocationError => !string.IsNullOrWhiteSpace(InstallLocationError);
    public bool HasCustomFieldError => !string.IsNullOrWhiteSpace(CustomFieldError);
    public bool HasIconPreview => IconPreview is not null;
    public bool MissingIconPreview => !HasIconPreview;

    public bool IsResultModalVisible
    {
        get => isResultModalVisible;
        private set => SetProperty(ref isResultModalVisible, value);
    }

    public bool IsAdvancedSettingsModalVisible
    {
        get => isAdvancedSettingsModalVisible;
        private set => SetProperty(ref isAdvancedSettingsModalVisible, value);
    }

    public string CustomFieldsSummary => this.CustomFields.Count(item => !string.IsNullOrWhiteSpace(item.Key) || !string.IsNullOrWhiteSpace(item.Value)) switch
    {
        0 => "No custom fields configured.",
        1 => "1 custom field configured.",
        var count => $"{count} custom fields configured."
    };

    public string ResultModalTitle
    {
        get => resultModalTitle;
        private set => SetProperty(ref resultModalTitle, value);
    }

    public string ResultModalMessage
    {
        get => resultModalMessage;
        private set => SetProperty(ref resultModalMessage, value);
    }

    public bool ResultIsSuccess
    {
        get => resultIsSuccess;
        private set
        {
            if (SetProperty(ref resultIsSuccess, value))
            {
                RaisePropertyChanged(nameof(ResultModalAccentBrush));
            }
        }
    }

    public IBrush ResultModalAccentBrush => ResultIsSuccess ? Brushes.MediumSeaGreen : Brushes.IndianRed;

    public Bitmap? IconPreview
    {
        get => iconPreview;
        private set
        {
            if (SetProperty(ref iconPreview, value))
            {
                RaisePropertyChanged(nameof(HasIconPreview));
                RaisePropertyChanged(nameof(MissingIconPreview));
            }
        }
    }

    private async Task PickAppImageAsync()
    {
        var path = await filePickerService.PickAppImageAsync();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        SelectedAppImagePath = path;
        DisplayName = Path.GetFileNameWithoutExtension(path);
        AppImageError = string.Empty;
        Validate();
    }

    private async Task PickIconAsync()
    {
        var path = await filePickerService.PickIconAsync();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        SelectedIconPath = path;
        IconError = string.Empty;
        LoadIconPreview(path);
        Validate();
    }

    private async Task PickInstallDirectoryAsync()
    {
        var selectedPath = await filePickerService.PickInstallDirectoryAsync(InstallLocation);
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        if (!ValidateInstallDirectory(selectedPath, out var error))
        {
            InstallLocationError = error;
            return;
        }

        InstallLocation = selectedPath;
        InstallLocationError = string.Empty;
        await appSettingsService.SaveLastInstallDirectoryAsync(selectedPath);
        Validate();
    }

    private async Task InstallAsync()
    {
        if (!Validate())
        {
            return;
        }

        IsInstalling = true;

        try
        {
            var result = await installerService.InstallAsync(new InstallRequest(
                SelectedAppImagePath!,
                SelectedIconPath!,
                InstallLocation,
                DisplayName.Trim(),
                SelectedCategory,
                BuildEffectiveCustomFields()));

            await appSettingsService.SaveLastInstallDirectoryAsync(InstallLocation);

            ShowResultModal(
                true,
                "Installation completed",
                $"AppImage: {result.InstalledAppImagePath}\nIcon: {result.InstalledIconPath}\nDesktop: {result.DesktopEntryPath}");
        }
        catch (Exception ex)
        {
            ShowResultModal(false, "Installation failed", ex.Message);
        }
        finally
        {
            IsInstalling = false;
        }
    }

    private bool Validate()
    {
        AppImageError = string.IsNullOrWhiteSpace(SelectedAppImagePath) ? "Select an AppImage file." : string.Empty;
        IconError = string.IsNullOrWhiteSpace(SelectedIconPath) ? "Select an icon file." : string.Empty;
        DisplayNameError = string.IsNullOrWhiteSpace(DisplayName) ? "Enter a display name." : string.Empty;
        CategoryError = string.IsNullOrWhiteSpace(SelectedCategory) ? "Choose a category." : string.Empty;

        InstallLocationError = ValidateInstallDirectory(InstallLocation, out var folderError)
            ? string.Empty
            : folderError;

        ValidateCustomFields();
        RaisePropertyChanged(nameof(CanInstall));
        RaiseCommands();

        return string.IsNullOrEmpty(AppImageError) &&
               string.IsNullOrEmpty(IconError) &&
               string.IsNullOrEmpty(DisplayNameError) &&
               string.IsNullOrEmpty(CategoryError) &&
               string.IsNullOrEmpty(InstallLocationError) &&
               string.IsNullOrEmpty(CustomFieldError);
    }

    private void AddCustomField()
    {
        CustomFields.Add(new DesktopCustomFieldInput());
        ValidateCustomFields();
        RaisePropertyChanged(nameof(CustomFieldsSummary));
    }

    private IReadOnlyList<DesktopCustomField> BuildEffectiveCustomFields()
    {
        var fields = new List<DesktopCustomField>
        {
            new("Version", StandardVersionOverride),
            new("Type", StandardTypeOverride),
            new("Name", StandardNameOverride),
            new("Exec", StandardExecOverride),
            new("Icon", StandardIconOverride),
            new("Categories", StandardCategoriesOverride),
            new("Terminal", StandardTerminalOverride)
        };

        foreach (var customField in CustomFields)
        {
            var key = customField.Key.Trim();
            var value = customField.Value.Trim();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            fields.Add(new DesktopCustomField(key, value));
        }

        return fields;
    }

    private void RemoveCustomField(object? parameter)
    {
        if (parameter is DesktopCustomFieldInput field)
        {
            CustomFields.Remove(field);
            ValidateCustomFields();
            RaisePropertyChanged(nameof(CustomFieldsSummary));
        }
    }

    private void ValidateCustomFields()
    {
        foreach (var field in CustomFields)
        {
            var key = field.Key.Trim();
            var value = field.Value.Trim();
            var hasKey = !string.IsNullOrWhiteSpace(key);
            var hasValue = !string.IsNullOrWhiteSpace(value);

            if (!hasKey && !hasValue)
            {
                continue;
            }

            if (!hasKey || !hasValue)
            {
                CustomFieldError = "Fill both key and value for each custom field.";
                return;
            }

            if (key.Any(char.IsWhiteSpace) || key.Contains('\n') || key.Contains('\r'))
            {
                CustomFieldError = "Custom field key cannot contain spaces or line breaks.";
                return;
            }

            if (value.Contains('\n') || value.Contains('\r'))
            {
                CustomFieldError = "Custom field value cannot contain line breaks.";
                return;
            }
        }

        CustomFieldError = string.Empty;
    }

    private static bool ValidateInstallDirectory(string path, out string error)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Select an installation folder.";
            return false;
        }

        if (!Path.IsPathRooted(path))
        {
            error = "Installation folder must be an absolute path.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(path);
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Cannot use selected folder: {ex.Message}";
            return false;
        }
    }

    private void InitializeInstallLocation()
    {
        var saved = appSettingsService.LoadLastInstallDirectory();
        if (!string.IsNullOrWhiteSpace(saved) && ValidateInstallDirectory(saved, out _))
        {
            InstallLocation = saved;
            return;
        }

        InstallLocation = DefaultInstallLocation;
    }

    private void ResetInstallDirectory()
    {
        InstallLocation = DefaultInstallLocation;
        InstallLocationError = string.Empty;
        Validate();
    }

    private void RaiseCommands()
    {
        if (PickAppImageCommand is AsyncCommand pickAppImageCommand)
        {
            pickAppImageCommand.RaiseCanExecuteChanged();
        }

        if (PickIconCommand is AsyncCommand pickIconCommand)
        {
            pickIconCommand.RaiseCanExecuteChanged();
        }

        if (PickInstallDirectoryCommand is AsyncCommand pickInstallDirectoryCommand)
        {
            pickInstallDirectoryCommand.RaiseCanExecuteChanged();
        }

        if (InstallCommand is AsyncCommand installCommand)
        {
            installCommand.RaiseCanExecuteChanged();
        }

        if (ResetInstallDirectoryCommand is RelayCommand resetInstallDirectoryCommand)
        {
            resetInstallDirectoryCommand.RaiseCanExecuteChanged();
        }

        if (AddCustomFieldCommand is RelayCommand addCustomFieldCommand)
        {
            addCustomFieldCommand.RaiseCanExecuteChanged();
        }

        if (RemoveCustomFieldCommand is RelayCommand removeCustomFieldCommand)
        {
            removeCustomFieldCommand.RaiseCanExecuteChanged();
        }

        if (OpenAdvancedSettingsModalCommand is RelayCommand openAdvancedSettingsModalCommand)
        {
            openAdvancedSettingsModalCommand.RaiseCanExecuteChanged();
        }

        if (CloseAdvancedSettingsModalCommand is RelayCommand closeAdvancedSettingsModalCommand)
        {
            closeAdvancedSettingsModalCommand.RaiseCanExecuteChanged();
        }
    }

    private void LoadIconPreview(string path)
    {
        iconPreview?.Dispose();

        try
        {
            IconPreview = new Bitmap(path);
        }
        catch
        {
            IconPreview = null;
        }
    }

    private void SetTheme(string themeKey)
    {
        if (selectedThemeKey == themeKey)
        {
            return;
        }

        selectedThemeKey = themeKey;
        applyTheme(themeKey);
        RaisePropertyChanged(nameof(ThemeGlyph));
        RaisePropertyChanged(nameof(ThemeLabel));
    }

    private void ToggleTheme()
    {
        SetTheme(selectedThemeKey == "Dark" ? "Light" : "Dark");
    }

    private void ShowResultModal(bool isSuccess, string title, string message)
    {
        ResultIsSuccess = isSuccess;
        ResultModalTitle = title;
        ResultModalMessage = message;
        IsResultModalVisible = true;
    }

    private void CloseResultModal()
    {
        IsResultModalVisible = false;
    }

    private void OpenAdvancedSettingsModal()
    {
        IsAdvancedSettingsModalVisible = true;
    }

    private void CloseAdvancedSettingsModal()
    {
        IsAdvancedSettingsModalVisible = false;
        CustomFieldError = string.Empty;
    }
}
