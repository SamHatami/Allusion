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

    public SettingsViewModel(AllusionConfiguration configuration, IThemeService themes, IEventAggregator events)
    {
        _configuration = configuration;
        _themes = themes;
        _events = events;
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
