using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Allusion.Tests;

public partial class ThemeResourcesTests
{
    [GeneratedRegex("<Color\\s+x:Key=\"([^\"]+)\"", RegexOptions.Compiled)]
    private static partial Regex ColorKeyRegex();

    [GeneratedRegex("<SolidColorBrush\\s+x:Key=\"([^\"]+)\"", RegexOptions.Compiled)]
    private static partial Regex BrushKeyRegex();

    [Fact]
    public void AllThemeDictionaries_ShouldDefineIdenticalColorKeys()
    {
        var keySets = LoadKeySets(ColorKeyRegex());

        var reference = keySets["Dark"];
        reference.Should().NotBeEmpty();
        foreach (var (name, keys) in keySets)
            keys.Should().BeEquivalentTo(reference, $"{name}.xaml must define the same color keys (missing keys crash DynamicResource on .NET 10)");
    }

    [Fact]
    public void AllThemeDictionaries_ShouldDefineIdenticalBrushKeys()
    {
        var keySets = LoadKeySets(BrushKeyRegex());

        var reference = keySets["Dark"];
        reference.Should().NotBeEmpty("each theme must fully describe the palette with literal brushes");
        foreach (var (name, keys) in keySets)
            keys.Should().BeEquivalentTo(reference, $"{name}.xaml must define the same brush keys so every control re-themes on switch");
    }

    private static Dictionary<string, HashSet<string>> LoadKeySets(Regex regex)
    {
        var themesDir = FindThemesDir();
        var service = new Allusion.WPFCore.Service.ThemeService(new Allusion.WPFCore.AllusionConfiguration());
        service.AvailableThemes.Should().HaveCountGreaterThanOrEqualTo(4, "expected Dark, Light, Midnight, Sand");

        return service.AvailableThemes.ToDictionary(
            name => name,
            name => regex.Matches(File.ReadAllText(Path.Combine(themesDir, $"{name}.xaml"))).Select(m => m.Groups[1].Value).ToHashSet());
    }

    private static string FindThemesDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "Allusion", "Themes");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Allusion/Themes from test output");
    }
}
