using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Allusion.WPFCore.Board;

public class ArtBoard
{
    public string Name { get; set; }
    public string FullPath { get; set; }

    public List<BoardPage> Pages { get; set; } = new List<BoardPage>();
    public List<ImageItem> Images { get; set; } = new List<ImageItem>();

    public ArtBoard(string name, string fullPath)
    {
        Name = name;
        FullPath = fullPath;
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
        var folderpath = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(folderpath))
            Directory.CreateDirectory(folderpath);

        Trace.WriteLine(folderpath);
        File.WriteAllText(fullPath, JsonSerializer.Serialize(board));
    }
}