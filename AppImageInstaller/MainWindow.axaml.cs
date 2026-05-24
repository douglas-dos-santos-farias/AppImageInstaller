using AppImageInstaller.Services;
using AppImageInstaller.ViewModels;
using Avalonia;
using Avalonia.Controls;

namespace AppImageInstaller;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var pickerService = new AvaloniaFilePickerService(this);
        var desktopEntryWriter = new DesktopEntryWriter();
        var installerService = new AppImageInstallerService(desktopEntryWriter);
        var settingsService = new AppSettingsService();
        var app = (App)Application.Current!;
        DataContext = new MainWindowViewModel(pickerService, installerService, settingsService, app.SetTheme);
    }
}
