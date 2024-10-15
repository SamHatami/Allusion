using Allusion.WPFCore.Board;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using Caliburn.Micro;
using System.IO;

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
    }

    public void RenamePage(BoardPage page, string newName)
    {
        if(string.IsNullOrEmpty(newName) ||string.Equals(page.Name,newName)) return;

        page.Name = newName;
        var directory = Path.GetFileName(page.PageFolder);
        var newDirectory = page.PageFolder.Replace(directory, newName);


        if (Directory.Exists(newDirectory))
        {
            //Implement status events that's handled in the mainview
            throw new IOException($"The directory '{newDirectory}' already exists.");
        }

        Directory.Move(page.PageFolder, newDirectory);

        page.BackupFolder = page.BackupFolder.Replace(directory, newName);
        page.PageFolder = newDirectory;

        foreach (var item in page.ImageItems)
        {
            item.ItemPath = item.ItemPath.Replace(directory, newDirectory);
        }
    }

}