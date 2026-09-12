using System.Diagnostics;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class UpdateViewModel : Screen
{
    private readonly IUpdateService _updateService;
    private bool _isChecking;
    private string _statusText = string.Empty;
    private UpdateInfo? _availableUpdate;

    public UpdateViewModel(IUpdateService updateService)
    {
        DisplayName = "Updates";
        _updateService = updateService;
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

    public UpdateInfo? AvailableUpdate
    {
        get => _availableUpdate;
        private set
        {
            if (Equals(_availableUpdate, value)) return;

            _availableUpdate = value;
            NotifyOfPropertyChange(nameof(AvailableUpdate));
            NotifyOfPropertyChange(nameof(HasUpdate));
        }
    }

    public bool HasUpdate => AvailableUpdate is not null;

    public bool CanCheckNow => !IsChecking;

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
        var url = AvailableUpdate?.DownloadUrl ?? AvailableUpdate?.PageUrl;
        if (string.IsNullOrEmpty(url)) return;

        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }

    private static string GetCurrentVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev";
    }
}
