using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Service;

public class ThemeService : IThemeService
{
    private readonly AllusionConfiguration _configuration;
    private string _currentTheme;
    private ResourceDictionary? _themeDictionary;

    public ThemeService(AllusionConfiguration configuration)
    {
        _configuration = configuration;
        _currentTheme = AvailableThemes.Contains(configuration.Theme)
            ? configuration.Theme
            : AvailableThemes[0];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<string> AvailableThemes { get; } = Themes;

    private static readonly string[] Themes = ["Dark", "Light", "Midnight", "Sand"];

    public string CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme == value) return;

            _currentTheme = value;
            OnPropertyChanged();
        }
    }

    public void ApplyTheme(string name)
    {
        if (!AvailableThemes.Contains(name))
            return;

        var dictionaries = Application.Current?.Resources.MergedDictionaries;
        _themeDictionary ??= dictionaries?.FirstOrDefault(d =>
            d.Source is not null && AvailableThemes.Any(t =>
                d.Source.OriginalString.EndsWith($"{t}.xaml", StringComparison.OrdinalIgnoreCase)));
        if (_themeDictionary is not null && dictionaries is not null)
        {
            var fresh = (ResourceDictionary)Application.LoadComponent(
                new Uri($"/Allusion;component/Themes/{name}.xaml", UriKind.Relative));
            dictionaries[dictionaries.IndexOf(_themeDictionary)] = fresh;
            _themeDictionary = fresh;
        }

        CurrentTheme = name;
        _configuration.Theme = name;
        AllusionConfiguration.Save(_configuration);
    }

    public void CycleTheme()
    {
        var next = (Array.IndexOf(Themes, CurrentTheme) + 1) % Themes.Length;
        ApplyTheme(Themes[next]);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
