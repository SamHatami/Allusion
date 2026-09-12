using System.ComponentModel;

namespace Allusion.WPFCore.Interfaces;

public interface IThemeService : INotifyPropertyChanged
{
    IReadOnlyList<string> AvailableThemes { get; }
    string CurrentTheme { get; }
    void ApplyTheme(string name);
    void CycleTheme();
}
