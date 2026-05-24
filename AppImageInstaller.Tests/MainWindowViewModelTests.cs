using AppImageInstaller.Models;
using AppImageInstaller.Services;
using AppImageInstaller.ViewModels;
using Xunit;

namespace AppImageInstaller.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void AddCustomField_AddsItem_WhenInputIsValid()
    {
        var vm = CreateViewModel();

        vm.CustomFieldKey = "X-Test";
        vm.CustomFieldValue = "123";
        vm.AddCustomFieldCommand.Execute(null);

        Assert.Single(vm.CustomFields);
        Assert.Equal("X-Test", vm.CustomFields[0].Key);
        Assert.Equal("123", vm.CustomFields[0].Value);
        Assert.False(vm.HasCustomFieldError);
    }

    [Fact]
    public void AddCustomField_RejectsEmptyKeyOrValue()
    {
        var vm = CreateViewModel();

        vm.CustomFieldValue = "123";
        vm.AddCustomFieldCommand.Execute(null);
        Assert.True(vm.HasCustomFieldError);

        vm.CustomFieldKey = "X-Test";
        vm.CustomFieldValue = "";
        vm.AddCustomFieldCommand.Execute(null);

        Assert.True(vm.HasCustomFieldError);
        Assert.Empty(vm.CustomFields);
    }

    [Fact]
    public void RemoveCustomField_RemovesItem()
    {
        var vm = CreateViewModel();
        vm.CustomFieldKey = "X-Test";
        vm.CustomFieldValue = "123";
        vm.AddCustomFieldCommand.Execute(null);

        var field = vm.CustomFields[0];
        vm.RemoveCustomFieldCommand.Execute(field);

        Assert.Empty(vm.CustomFields);
    }

    [Fact]
    public async Task Install_UsesCurrentCustomFields_AndAlsoWorksWithoutCustomFields()
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
        Assert.Empty(installer.LastRequest!.CustomFields);

        vm.CustomFieldKey = "Exec";
        vm.CustomFieldValue = "/tmp/override.AppImage";
        vm.AddCustomFieldCommand.Execute(null);

        installer.LastRequest = null;
        vm.InstallCommand.Execute(null);
        await WaitUntilAsync(() => installer.LastRequest is not null);

        Assert.NotNull(installer.LastRequest);
        Assert.Single(installer.LastRequest!.CustomFields);
        Assert.Equal("Exec", installer.LastRequest.CustomFields[0].Key);
        Assert.Equal("/tmp/override.AppImage", installer.LastRequest.CustomFields[0].Value);
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
