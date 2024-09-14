using Allusion.WPFCore.Service;
using System.IO;
using System.Text.Json;

namespace Allusion.WPFCore.Board;

public class ArtBoard
{
    public string Name { get; set; }
    public string ImageFolder { get; set; }

    public List<BoardPage> Pages { get; set; } = new();
    public List<ImageItem> Images { get; set; } = new();

    public ArtBoard(string name, string imageFolder)
    {
        Name = name;
        ImageFolder = imageFolder;
    }

    public static ArtBoard? Read(string projectFileFullPath)
    {
        if (!File.Exists(projectFileFullPath)) return null;

        var projectJson = File.ReadAllText(projectFileFullPath);

        var project = JsonSerializer.Deserialize<ArtBoard>(projectJson);

        return project;
    }

    public static void Save(ArtBoard board)
    {
        if (!Directory.Exists(AllusionConfiguration.DataFolder))
            Directory.CreateDirectory(AllusionConfiguration.DataFolder);

        var artboardDataFile = Path.Combine(AllusionConfiguration.DataFolder, board.Name + ".json");

        File.WriteAllText(artboardDataFile, JsonSerializer.Serialize(board));

        SaveImages(board);
    }

    private static void SaveImages(ArtBoard board)
    {
        if (!Directory.Exists(board.ImageFolder))
            Directory.CreateDirectory(board.ImageFolder);

        var i = 0;
        foreach (var image in board.Images)
        {
            var fullFileNameWithoutExtension = Path.Combine(board.ImageFolder, i.ToString());
            BitmapService.SaveToFile(image.SourceImage, fullFileNameWithoutExtension);

            i++;
        }
    }
}