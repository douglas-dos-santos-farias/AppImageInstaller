using AppImageInstaller.Models;
using AppImageInstaller.Services;
using AppImageInstaller.ViewModels;
using Xunit;

namespace AppImageInstaller.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void AddCustomField_AddsEditableRow()
    {
        var vm = CreateViewModel();

        vm.AddCustomFieldCommand.Execute(null);

        Assert.Single(vm.CustomFields);
        Assert.Equal(string.Empty, vm.CustomFields[0].Key);
        Assert.Equal(string.Empty, vm.CustomFields[0].Value);
    }

    [Fact]
    public async Task Install_RejectsPartialCustomFieldRow()
    {
        var vm = CreateViewModel();

        vm.AddCustomFieldCommand.Execute(null);
        vm.CustomFields[0].Key = "X-Test";

        vm.PickAppImageCommand.Execute(null);
        vm.PickIconCommand.Execute(null);
        await WaitUntilAsync(() => vm.SelectedAppImagePath is not null && vm.SelectedIconPath is not null);
        vm.DisplayName = "Sample App";
        vm.SelectedCategory = "Utility";

        vm.InstallCommand.Execute(null);
        await Task.Delay(100);

        Assert.True(vm.HasCustomFieldError);
    }

    [Fact]
    public void RemoveCustomField_RemovesItem()
    {
        var vm = CreateViewModel();
        vm.AddCustomFieldCommand.Execute(null);
        vm.CustomFields[0].Key = "X-Test";
        vm.CustomFields[0].Value = "123";

        var field = vm.CustomFields[0];
        vm.RemoveCustomFieldCommand.Execute(field);

        Assert.Empty(vm.CustomFields);
    }

    [Fact]
    public async Task Install_UsesStandardOverrides_AndCustomFields()
    {
        var installer = new FakeInstallerService();
        var vm = CreateViewModel(installer);

        vm.PickAppImageCommand.Execute(null);
        vm.PickIconCommand.Execute(null);
        await WaitUntilAsync(() => vm.SelectedAppImagePath is not null && vm.SelectedIconPath is not null);

        vm.DisplayName = "Sample App";
        vm.SelectedCategory = "Utility";

        vm.InstallCommand.Execute(null);
        await WaitUntilAsync(() => installer.LastRequest is not null);

        Assert.NotNull(installer.LastRequest);
        Assert.Contains(installer.LastRequest!.CustomFields, field => field.Key == "Version" && field.Value == "1.0");
        Assert.Contains(installer.LastRequest.CustomFields, field => field.Key == "Type" && field.Value == "Application");
        Assert.Contains(installer.LastRequest.CustomFields, field => field.Key == "Name" && field.Value == "Sample App");
        Assert.Contains(installer.LastRequest.CustomFields, field => field.Key == "Categories" && field.Value == "Utility");
        Assert.Contains(installer.LastRequest.CustomFields, field => field.Key == "Terminal" && field.Value == "false");

        vm.StandardExecOverride = "/opt/my-app.AppImage";
        vm.StandardIconOverride = "/opt/my-app.png";
        vm.AddCustomFieldCommand.Execute(null);
        vm.CustomFields[0].Key = "Exec";
        vm.CustomFields[0].Value = "/tmp/override.AppImage";

        installer.LastRequest = null;
        vm.InstallCommand.Execute(null);
        await WaitUntilAsync(() => installer.LastRequest is not null);

        Assert.NotNull(installer.LastRequest);
        Assert.Contains(installer.LastRequest!.CustomFields, field => field.Key == "Exec" && field.Value == "/opt/my-app.AppImage");
        Assert.Equal("Exec", installer.LastRequest.CustomFields[^1].Key);
        Assert.Equal("/tmp/override.AppImage", installer.LastRequest.CustomFields[^1].Value);
    }

    private static MainWindowViewModel CreateViewModel(IAppImageInstallerService? installer = null)
        => new(
            new FakeFilePickerService(),
            installer ?? new FakeInstallerService(),
            new FakeAppSettingsService(),
            _ => { });

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        var timeout = TimeSpan.FromSeconds(2);
        var start = DateTime.UtcNow;

        while (!predicate())
        {
            if (DateTime.UtcNow - start > timeout)
            {
                throw new TimeoutException("Condition was not met in time.");
            }

            await Task.Delay(20);
        }
    }

    private sealed class FakeFilePickerService : IFilePickerService
    {
        public Task<string?> PickAppImageAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("/tmp/app.AppImage");

        public Task<string?> PickIconAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("/tmp/icon.png");

        public Task<string?> PickInstallDirectoryAsync(string? startLocation = null, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("/tmp");
    }

    private sealed class FakeAppSettingsService : IAppSettingsService
    {
        public string? LoadLastInstallDirectory() => "/tmp";

        public Task SaveLastInstallDirectoryAsync(string path, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeInstallerService : IAppImageInstallerService
    {
        public InstallRequest? LastRequest { get; set; }

        public Task<InstallResult> InstallAsync(InstallRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new InstallResult("/tmp/app", "/tmp/icon", "/tmp/app.desktop"));
        }
    }
}
