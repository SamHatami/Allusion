using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Allusion.WPFCore;

[Serializable]
public class AllusionConfiguration
{
    private const string ConfigFileName = "AllusionConfiguration.json";

    public bool FirstStartUp { get; set; } = true;
    public bool TopMost { get; set; } = true;
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
        set => _globalFolder = value;
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
            CreateNew();

        var rawFile = File.ReadAllText(ConfigPath);
        var configuration = JsonSerializer.Deserialize<AllusionConfiguration>(rawFile);

        return configuration ?? new AllusionConfiguration();
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
