using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Allusion.WPFCore;

[Serializable]
public class AllusionConfiguration : INotifyPropertyChanged
{
    private const string ConfigFileName = "AllusionConfiguration.json";

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _firstStartUp = true;

    public bool FirstStartUp
    {
        get => _firstStartUp;
        set => SetField(ref _firstStartUp, value);
    }

    private bool _topMost = true;

    public bool TopMost
    {
        get => _topMost;
        set => SetField(ref _topMost, value);
    }

    private string _theme = "Dark";

    public string Theme
    {
        get => _theme;
        set => SetField(ref _theme, string.IsNullOrWhiteSpace(value) ? "Dark" : value);
    }

    private double _imageBorderThickness = 4;

    public double ImageBorderThickness
    {
        get => _imageBorderThickness;
        set => SetField(ref _imageBorderThickness, Math.Clamp(value, 0, 12));
    }
    public List<string> IgnoredRefBoardFiles { get; set; } = [];

    private string _globalFolder = string.Empty;

    public string GlobalFolder
    {
        get
        {
            var result = string.IsNullOrEmpty(_globalFolder) ? DefaultFolder : _globalFolder;
            if (!Directory.Exists(result))
                Directory.CreateDirectory(result);
            Trace.WriteLine($"GlobalFolder get: {result}");
            return result;
        }
        set
        {
            if (_globalFolder == value) return;
            _globalFolder = value;
            OnPropertyChanged();
        }
    }

    [JsonIgnore]
    public static string DefaultFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Allusion");

    private static string _dataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Allusion");

    public static string DataFolder => _dataFolder;

    public static string ConfigPath => Path.Combine(DataFolder, ConfigFileName);

    public static AllusionConfiguration Read()
    {
        if (!File.Exists(ConfigPath))
        {
            CreateNew();
            return new AllusionConfiguration();
        }

        try
        {
            var rawFile = File.ReadAllText(ConfigPath);
            var configuration = JsonSerializer.Deserialize<AllusionConfiguration>(rawFile);
            return configuration ?? new AllusionConfiguration();
        }
        catch (Exception)
        {
            return new AllusionConfiguration();
        }
    }

    public static void Save(AllusionConfiguration config)
    {
        Directory.CreateDirectory(DataFolder);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config));
    }

    private static void CreateNew()
    {
        Save(new AllusionConfiguration());
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal static IDisposable UseDataFolderForTests(string dataFolder)
    {
        var previousDataFolder = _dataFolder;
        _dataFolder = dataFolder;
        return new DataFolderScope(previousDataFolder);
    }

    private sealed class DataFolderScope : IDisposable
    {
        private readonly string _previousDataFolder;

        public DataFolderScope(string previousDataFolder)
        {
            _previousDataFolder = previousDataFolder;
        }

        public void Dispose()
        {
            _dataFolder = _previousDataFolder;
        }
    }
}
