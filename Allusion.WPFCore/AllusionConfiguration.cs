﻿using System.Diagnostics;
using Allusion.WPFCore.Interfaces;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Allusion.WPFCore;

[Serializable]
public class AllusionConfiguration : IConfiguration
{
    private string _globalFolder = string.Empty;

    public string GlobalFolder
    {
        get
        {
            var result = string.IsNullOrEmpty(_globalFolder) ? DefaultFolder : _globalFolder;
            Trace.WriteLine($"GlobalFolder get: {result}");
            return result;
        }
        set => _globalFolder = value;
    }

    [JsonIgnore]
    public static string DefaultFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Allusion");

    public static string DataFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Allusion");

    private static string _configPath = Path.Combine(Directory.GetCurrentDirectory(), "IConfiguration.json");

    public static IConfiguration Read()
    {
        if (!File.Exists(_configPath)) CreateNew();

        var rawFile = File.ReadAllText(_configPath);
        var configuration = JsonSerializer.Deserialize<IConfiguration>(rawFile);

        return configuration;
    }

    public static void Save(IConfiguration config)
    {
        File.WriteAllText(_configPath, JsonSerializer.Serialize(config));
    }

    private static void CreateNew()
    {
        var configuration = new AllusionConfiguration();

        File.WriteAllText(_configPath, JsonSerializer.Serialize(configuration));
    }
}