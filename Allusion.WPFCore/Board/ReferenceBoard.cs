using Allusion.WPFCore.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Allusion.WPFCore.Service;

namespace Allusion.WPFCore.Board;

[Serializable]
public class ReferenceBoard
{
    public string Name { get; set; }
    public string BaseFolder { get; set; }
    public string BackupFolder { get; set; }

    public List<BoardPage> Pages { get; set; } = [];

    public ReferenceBoard(string name, string baseFolder)
    {
        Name = name;
        BaseFolder = baseFolder;

        Initialize();
    }

    private void Initialize()
    {
        BackupFolder = Path.Combine(BaseFolder, "Old");
        Directory.CreateDirectory(BaseFolder);
        Directory.CreateDirectory(Path.Combine(BackupFolder));

        if (Pages.Count == 0)
            Pages.Add(new BoardPage(this));
    }

    public static ReferenceBoard? Read(string projectFileFullPath)
    {
        if (!File.Exists(projectFileFullPath)) return null;

        var projectJson = File.ReadAllText(projectFileFullPath);
        ReferenceBoard project = null;

        try
        {
            project = JsonSerializer.Deserialize<ReferenceBoard>(projectJson);
        }
        catch (Exception e)
        {
            StaticLogger.Error("Could not open board", true, e.Message);
        }

        return project;
    }

    public static bool Save(ReferenceBoard board)
    {
        bool saveSuccess;
        if (!Directory.Exists(board.BaseFolder))
            Directory.CreateDirectory(board.BaseFolder);

        var RefBoardDataFile = Path.Combine(board.BaseFolder, board.Name + ".allusion");

        try
        {
            File.WriteAllText(RefBoardDataFile, JsonSerializer.Serialize(board));
            foreach (var page in board.Pages)
            {
                BackupRemovedImages(page);
                SaveImagesToDisc(page);
            }

            saveSuccess = true;
        }
        catch (Exception e)
        {
            StaticLogger.Error("Could not save board", true, e.Message);
            saveSuccess = false;
        }

        return saveSuccess;
    }

    private static void BackupRemovedImages(BoardPage page)
    {
        var imagePaths = GetImagePathsFromFolder(page);

        if(imagePaths == null)
        {
            Trace.WriteLine("imagepath was null");
            return;
        }
        var items = page.ImageItems.Select(i => i.ItemPath).ToArray();
        foreach (var path in imagePaths)
        {
            if (items.Contains(path)) continue;

            var toFile = GetBackupFilename(page.BackupFolder, path);

            try
            {
                File.Move(path, toFile);

            }
            catch (Exception e)
            {
                StaticLogger.Warning("Could not backup images", true);
            }
        }
    }

    private static string GetBackupFilename(string backUpFolder, string originalFilePath)
    {
        var id = Guid.NewGuid().ToString();
        var timestamp = DateTime.Now.ToShortDateString();
        var fileName = $"{timestamp}_{id}_{Path.GetFileName(originalFilePath)}";
        return Path.Combine(backUpFolder, fileName);
    }

    private static void CreateOldFolder(ReferenceBoard board)
    {
        if (!Directory.Exists(board.BackupFolder))
            Directory.CreateDirectory(board.BackupFolder);
    }

    private static string[] GetImagePathsFromFolder(BoardPage page)
    {
        var allFiles = Directory.GetFiles(page.PageFolder);
        var imagePaths = allFiles.Where(f => f.EndsWith(".png")).ToArray();

        return imagePaths;
    }

    private static void SaveImagesToDisc(BoardPage page)
    {
        if (!Directory.Exists(page.PageFolder))
        {
            Directory.CreateDirectory(page.PageFolder);
            Directory.CreateDirectory(Path.Combine(page.BackupFolder));
        }

        foreach (var image in page.ImageItems)
            BitmapUtils.SaveToFile(image.SourceImage, image.ItemPath);
    }
}

public enum BoardState
{
    Idle,
    Saving,
    Loading,
    Modified
}