using System.Diagnostics;
using Allusion.WPFCore.Service;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Documents;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Utilities;
using Microsoft.VisualBasic;

namespace Allusion.WPFCore.Board;

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
        BackupFolder = Path.Combine(BaseFolder, "Old");

        Directory.CreateDirectory(BaseFolder);
        Directory.CreateDirectory(Path.Combine(BackupFolder));

        IntializePages();
    }

    private void IntializePages()
    {
        if (Pages.Count == 0)
            Pages.Add(new BoardPage(this));
        
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
            foreach (var page in board.Pages)
            {
                RemoveDeletedImages(page);
                SaveImagesToDisc(page);
            }
            saveSuccess = true;
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            throw;
        }

        return saveSuccess;
    }

    private static void RemoveDeletedImages(BoardPage page)
    {
        var imagePaths = GetImagePathsFromFolder(page);
        var items = page.ImageItems.Select(i => i.ItemPath).ToArray();
        foreach (var path in imagePaths)
        {
            if(items.Contains(path)) continue;

            //TODO: Create separate fileName helper ?  for both creation and backup
            var toFile = Path.Combine(
                page.BackupFolder, 
                DateTime.Today.ToShortDateString()+"_"+
                Path.GetFileName(path));

            File.Move(path,toFile);
        }
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