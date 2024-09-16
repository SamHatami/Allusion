using System.Diagnostics;
using Allusion.WPFCore.Service;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Allusion.WPFCore.Board;

public class ReferenceBoard
{
    public string Name { get; set; }
    public string BaseFolder { get; set; }

    public List<BoardPage> Pages { get; set; } = new();
    public List<ImageItem> Images { get; set; } = new();

   
    public ReferenceBoard(string name, string baseFolder)
    {
        Name = name;
        BaseFolder = baseFolder;
    }

    public static ReferenceBoard? Read(string projectFileFullPath)
    {
        if (!File.Exists(projectFileFullPath)) return null;

        var projectJson = File.ReadAllText(projectFileFullPath);

        var project = JsonSerializer.Deserialize<ReferenceBoard>(projectJson);

        return project;
    }

    public static bool Save(ReferenceBoard board)
    {
        bool saveSuccess;
        if (!Directory.Exists(board.BaseFolder))
            Directory.CreateDirectory(board.BaseFolder);

        var RefBoardDataFile = Path.Combine(board.BaseFolder, board.Name + ".json");

        try
        {
            File.WriteAllText(RefBoardDataFile, JsonSerializer.Serialize(board));
            SaveImagesToDisc(board);
            saveSuccess = true;
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            throw;
        }

        return saveSuccess;
    }

    public static void Rename(ReferenceBoard board, string newName)
    {

    }

    private static void SaveImagesToDisc(ReferenceBoard board)
    {
        if (!Directory.Exists(board.BaseFolder))
            Directory.CreateDirectory(board.BaseFolder);

        foreach (var image in board.Images)
            BitmapService.SaveToFile(image.SourceImage, image.ImagePath);
    }
}