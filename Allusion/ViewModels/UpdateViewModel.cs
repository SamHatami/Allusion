using System.Diagnostics;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class UpdateViewModel : Screen
{
    private readonly IUpdateService _updateService;
    private readonly IUpdateInstaller _installer;
    private bool _isChecking;
    private bool _isInstalling;
    private string _statusText = string.Empty;
    private UpdateInfo? _availableUpdate;

    public UpdateViewModel(IUpdateService updateService, IUpdateInstaller installer)
    {
        DisplayName = "Updates";
        _updateService = updateService;
        _installer = installer;
        CurrentVersion = GetCurrentVersion();
        StatusText = $"Current version: {CurrentVersion}";
    }

    public string CurrentVersion { get; }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            if (_statusText == value) return;

            _statusText = value;
            NotifyOfPropertyChange(nameof(StatusText));
        }
    }

    public bool IsChecking
    {
        get => _isChecking;
        private set
        {
            if (_isChecking == value) return;

            _isChecking = value;
            NotifyOfPropertyChange(nameof(IsChecking));
            NotifyOfPropertyChange(nameof(CanCheckNow));
        }
    }

    public bool IsInstalling
    {
        get => _isInstalling;
        private set
        {
            if (_isInstalling == value) return;

            _isInstalling = value;
            NotifyOfPropertyChange(nameof(IsInstalling));
            NotifyOfPropertyChange(nameof(CanCheckNow));
            NotifyOfPropertyChange(nameof(CanOpenDownload));
        }
    }

    public UpdateInfo? AvailableUpdate
    {
        get => _availableUpdate;
        private set
        {
            if (Equals(_availableUpdate, value)) return;

            _availableUpdate = value;
            NotifyOfPropertyChange(nameof(AvailableUpdate));
            NotifyOfPropertyChange(nameof(HasUpdate));
            NotifyOfPropertyChange(nameof(CanOpenDownload));
        }
    }

    public bool HasUpdate => AvailableUpdate is not null;

    public bool CanCheckNow => !IsChecking && !IsInstalling;

    public bool CanOpenDownload => HasUpdate && !IsInstalling;

    public async Task CheckNow()
    {
        if (IsChecking) return;

        IsChecking = true;
        StatusText = "Checking for updates...";
        try
        {
            AvailableUpdate = await _updateService.CheckForUpdatesAsync().ConfigureAwait(true);
            StatusText = AvailableUpdate is null
                ? $"You're up to date ({CurrentVersion})"
                : $"Allusion {AvailableUpdate.Version} is available";
        }
        catch
        {
            AvailableUpdate = null;
            StatusText = "Couldn't reach the update server";
        }
        finally
        {
            IsChecking = false;
        }
    }

    public void OpenDownload()
    {
        if (AvailableUpdate is null || IsInstalling) return;

        _ = InstallAsync();
    }

    private async Task InstallAsync()
    {
        IsInstalling = true;
        StatusText = $"Downloading {AvailableUpdate!.Version}...";
        try
        {
            if (await _installer.DownloadAndInstallAsync().ConfigureAwait(true))
            {
                StatusText = "Restarting to finish the update...";
                return;
            }
        }
        finally
        {
            IsInstalling = false;
        }

        OpenInBrowser();
    }

    private void OpenInBrowser()
    {
        var url = AvailableUpdate?.DownloadUrl ?? AvailableUpdate?.PageUrl;
        if (string.IsNullOrEmpty(url)) return;

        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch
        {
            StatusText = "Couldn't open the download page";
        }
    }

    private static string GetCurrentVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev";
    }
}
