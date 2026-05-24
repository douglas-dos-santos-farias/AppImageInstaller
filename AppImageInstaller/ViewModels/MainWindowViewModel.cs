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
    private bool isResultModalVisible;
    private string resultModalTitle = string.Empty;
    private string resultModalMessage = string.Empty;
    private bool resultIsSuccess;
    private bool isInstalling;

    public MainWindowViewModel(IFilePickerService filePickerService, IAppImageInstallerService installerService, Action<string> applyTheme)
    {
        this.filePickerService = filePickerService;
        this.installerService = installerService;
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

        PickAppImageCommand = new AsyncCommand(PickAppImageAsync, () => !IsInstalling);
        PickIconCommand = new AsyncCommand(PickIconAsync, () => !IsInstalling);
        InstallCommand = new AsyncCommand(InstallAsync, () => !IsInstalling);
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        CloseResultModalCommand = new RelayCommand(CloseResultModal);
        applyTheme(selectedThemeKey);
    }

    public ObservableCollection<string> Categories { get; }

    public ICommand PickAppImageCommand { get; }

    public ICommand PickIconCommand { get; }

    public ICommand InstallCommand { get; }

    public ICommand ToggleThemeCommand { get; }

    public ICommand CloseResultModalCommand { get; }

    public string? SelectedAppImagePath
    {
        get => selectedAppImagePath;
        private set
        {
            if (SetProperty(ref selectedAppImagePath, value))
            {
                RaisePropertyChanged(nameof(AppImageFileName));
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
                RaisePropertyChanged(nameof(CanInstall));
            }
        }
    }

    public string ThemeGlyph => selectedThemeKey == "Dark" ? "☾" : "☀";

    public string ThemeLabel => selectedThemeKey switch
    {
        "Light" => "Light mode",
        "Dark" => "Dark mode",
        _ => "Light mode"
    };

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
                              !string.IsNullOrWhiteSpace(SelectedCategory);

    public string AppImageFileName => Path.GetFileName(SelectedAppImagePath) ?? "No AppImage selected";

    public string IconFileName => Path.GetFileName(SelectedIconPath) ?? "No icon selected";

    public bool HasAppImageError => !string.IsNullOrWhiteSpace(AppImageError);

    public bool HasIconError => !string.IsNullOrWhiteSpace(IconError);

    public bool HasDisplayNameError => !string.IsNullOrWhiteSpace(DisplayNameError);

    public bool HasCategoryError => !string.IsNullOrWhiteSpace(CategoryError);

    public bool HasIconPreview => IconPreview is not null;

    public bool MissingIconPreview => !HasIconPreview;

    public bool IsResultModalVisible
    {
        get => isResultModalVisible;
        private set => SetProperty(ref isResultModalVisible, value);
    }

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

    public string InstallLocation => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".local/share/applications");

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
                DisplayName.Trim(),
                SelectedCategory));

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

        RaisePropertyChanged(nameof(CanInstall));
        RaiseCommands();

        return string.IsNullOrEmpty(AppImageError) &&
               string.IsNullOrEmpty(IconError) &&
               string.IsNullOrEmpty(DisplayNameError) &&
               string.IsNullOrEmpty(CategoryError);
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

        if (InstallCommand is AsyncCommand installCommand)
        {
            installCommand.RaiseCanExecuteChanged();
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
}
