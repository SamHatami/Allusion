using System.Diagnostics;
using Allusion.WPFCore;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;

namespace Allusion.ViewModels.Dialogs;

public class SettingsViewModel : Screen
{
    private readonly AllusionConfiguration _configuration;
    private readonly IThemeService _themes;
    private readonly IEventAggregator _events;
    private readonly IUpdateService _updateService;
    private readonly IUpdateInstaller _installer;
    private bool _isChecking;
    private bool _isInstalling;
    private string _updateStatus = string.Empty;
    private UpdateInfo? _availableUpdate;

    public SettingsViewModel(
        AllusionConfiguration configuration,
        IThemeService themes,
        IEventAggregator events,
        IUpdateService updateService,
        IUpdateInstaller installer)
    {
        _configuration = configuration;
        _themes = themes;
        _events = events;
        _updateService = updateService;
        _installer = installer;
        _updateStatus = "Up to date";
    }

    public string Title => "Settings";

    public IReadOnlyList<string> Themes => _themes.AvailableThemes;

    public string SelectedTheme
    {
        get => _themes.CurrentTheme;
        set
        {
            if (value == _themes.CurrentTheme) return;

            _themes.ApplyTheme(value);
            NotifyOfPropertyChange(nameof(SelectedTheme));
        }
    }

    public bool TopMost
    {
        get => _configuration.TopMost;
        set
        {
            if (_configuration.TopMost == value) return;

            _configuration.TopMost = value;
            AllusionConfiguration.Save(_configuration);
            NotifyOfPropertyChange(nameof(TopMost));
        }
    }

    public double ImageBorderThickness
    {
        get => _configuration.ImageBorderThickness;
        set
        {
            if (Math.Abs(_configuration.ImageBorderThickness - value) < 0.001) return;

            _configuration.ImageBorderThickness = value;
            NotifyOfPropertyChange(nameof(ImageBorderThickness));
            _events.PublishOnUIThreadAsync(new SettingsChangedEvent(value));
            DebouncedSave();
        }
    }

    public Task Close()
    {
        _saveCts?.Cancel();
        AllusionConfiguration.Save(_configuration);
        return TryCloseAsync();
    }

    public string CurrentVersion =>
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev";

    public string UpdateStatus
    {
        get => _updateStatus;
        private set
        {
            if (_updateStatus == value) return;

            _updateStatus = value;
            NotifyOfPropertyChange(nameof(UpdateStatus));
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

    public bool CanCheckNow => !IsChecking && !IsInstalling;

    public bool CanOpenDownload => HasUpdate && !IsInstalling;

    public async Task CheckNow()
    {
        if (IsChecking) return;

        IsChecking = true;
        UpdateStatus = "Checking for updates...";
        try
        {
            AvailableUpdate = await _updateService.CheckForUpdatesAsync().ConfigureAwait(true);
            UpdateStatus = AvailableUpdate is null
                ? "Up to date"
                : $"Allusion {AvailableUpdate.Version} is available";
        }
        catch
        {
            AvailableUpdate = null;
            UpdateStatus = "Couldn't reach the update server";
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
        UpdateStatus = $"Downloading {AvailableUpdate!.Version}...";
        try
        {
            if (await _installer.DownloadAndInstallAsync().ConfigureAwait(true))
            {
                UpdateStatus = "Restarting to finish the update...";
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
            UpdateStatus = "Couldn't open the download page";
        }
    }

    private CancellationTokenSource? _saveCts;

    private async void DebouncedSave()
    {
        _saveCts?.Cancel();
        var cts = _saveCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(500, cts.Token).ConfigureAwait(false);
            AllusionConfiguration.Save(_configuration);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
