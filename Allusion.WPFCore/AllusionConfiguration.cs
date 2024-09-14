using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Documents;
using Microsoft.Win32;

namespace Allusion.WPFCore;

[Serializable]
public class AllusionConfiguration
{
    private string _globalFolder = string.Empty;
    public string GlobalFolder
    {
        get
        {
            var result = string.IsNullOrEmpty(_globalFolder) ? DefaultFolder : _globalFolder;
            Console.WriteLine($"GlobalFolder get: {result}");
            return result;
        }
        set => _globalFolder = value;
    }

    [JsonIgnore]
    public string DefaultFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Allusion");

    private static string _configPath = Path.Combine(Directory.GetCurrentDirectory(), "AllusionConfiguration.json");
    public static AllusionConfiguration Read()
    {
        if (!File.Exists(_configPath)) CreateNew();
        
        var rawFile = File.ReadAllText(_configPath); 
        var configuration = JsonSerializer.Deserialize<AllusionConfiguration>(rawFile);

        return configuration;
    }

    public static void Save(AllusionConfiguration config)
    {
        File.WriteAllText(_configPath, JsonSerializer.Serialize(config));
    }

    private static void CreateNew()
    {
        AllusionConfiguration configuration = new AllusionConfiguration();

        File.WriteAllText(_configPath, JsonSerializer.Serialize(configuration));

    }
}