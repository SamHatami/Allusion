using System.IO;
using System.Text.Json;

namespace Allusion.WPFCore.Board;

public class ArtBoard
{
    public string Name { get; set; }
    public string FullPath { get; set; }

    public BoardPage[] Pages { get; set; }
    public ImageItem[] Images { get; set; }

    public ArtBoard(string name)
    {
        Name = name;
    }

    public static ArtBoard? Read(string projectFileFullPath)
    {
        if (!File.Exists(projectFileFullPath)) return null;

        var projectJson = File.ReadAllText(projectFileFullPath);

        var project = JsonSerializer.Deserialize<ArtBoard>(projectJson);

        return project;
    }

    public static void Save(ArtBoard board, string fullPath)
    {
        var path = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        
        File.WriteAllText(fullPath, JsonSerializer.Serialize(board));
    }
}