using System.Diagnostics;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using Caliburn.Micro;
using System.IO;
using System.Windows.Controls;

namespace Allusion.WPFCore.Managers;

public class PageManager : IPageManager
{
    private readonly ImageItemService _imageItemService;
    private readonly IEventAggregator _events;

    public PageManager(IEventAggregator events, IClipboardService clipboardService)
    {
        _events = events;
        _imageItemService = new ImageItemService(events, clipboardService);
    }

    //Health check if there a hidden items or some other issues that might arrive in the future
    public void CleanPage(BoardPage page)
    {
        List<ImageItem> itemsToRemove = new List<ImageItem>();
        foreach (var item in page.ImageItems)
        {
            if (!File.Exists(item.ItemPath))
                itemsToRemove.Add(item);
        }

        foreach (var item in itemsToRemove)
        {
            page.ImageItems.Remove(item);

        }

    }

    public void AddImage(ImageItem imageItem, BoardPage page)
    {
        var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

        if (page.ImageItems.Contains(imageItem)) return;

        
        var fullFileName = Path.Combine(page.PageFolder, randomFileName + ".png");
        imageItem.ItemPath = fullFileName;
        imageItem.MemberOfPage = page.BoardId;
        page.ImageItems.Add(imageItem);

    }


    public void AddNoteToImage(ImageItem item, BoardPage page, NoteItem note)
    {
        //TODO: Something in the imageItemService
    }

    public void AddNote(BoardPage page, NoteItem note)
    {
    }

    public void RemoveImage(ImageItem imageItem, BoardPage page)
    {
        if (!page.ImageItems.Contains(imageItem)) return;

        page.ImageItems.Remove(imageItem);
        StaticLogger.Info("Removed image", false);
    }

    public void OpenPageFolder(BoardPage page)
    {
        if (Directory.Exists(page.PageFolder))
            Directory.CreateDirectory(page.PageFolder);

        try
        {

            var processInfo = new ProcessStartInfo
            {
                FileName = page.PageFolder,
                UseShellExecute = true
            };
            Process.Start(processInfo);

        }
        catch (Exception e)
        {
            StaticLogger.Error("Could not open folder");
            StaticLogger.WriteToLog(e.Message, StaticLogger.LogLevel.Error);
         
        }
    }


    public void RenamePage(BoardPage page, string newName)
    {
        //Illegal chars are expected to be catched before reaching this 
        if(string.IsNullOrEmpty(newName) ||string.Equals(page.Name,newName)) return;

        page.Name = newName;

        var oldDirectoryPath = page.PageFolder;
        var oldDirectoryName = Path.GetFileName(oldDirectoryPath);

        var parentDirectory = Path.GetDirectoryName(oldDirectoryPath);
        var newDirectory = Path.Combine(parentDirectory, newName);
        
        if (Directory.Exists(newDirectory)) //Replace with directoryHelper class
        {
            StaticLogger.Info($"The directory '{newDirectory}' already exists.", true, true);
        }

        if (!Directory.Exists(page.PageFolder))
            Directory.CreateDirectory(newDirectory);
        else
        {
            try
            {
                Directory.Move(page.PageFolder, newDirectory);
                StaticLogger.WriteToLog("Renamed", StaticLogger.LogLevel.Info);
            }
            catch (Exception e)
            {
                StaticLogger.Error("Something went wrong when renaming folder",true, e.Message);
            }

        }


        page.BackupFolder = page.BackupFolder.Replace(oldDirectoryName, newName);
        page.PageFolder = newDirectory;

        foreach (var item in page.ImageItems)
        {
            item.ItemPath = item.ItemPath.Replace(oldDirectoryName, newDirectory);
        }
    }

}