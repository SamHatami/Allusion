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
    
    public void AddImage(ImageItem imageItem, BoardPage page)
    {
        var nrFiles = Directory.GetFiles(page.PageFolder).Length;

        if (page.ImageItems.Contains(imageItem)) return;

        imageItem.MemberOfPage = page.BoardId;
        page.ImageItems.Add(imageItem);
        var fullFileName = Path.Combine(page.PageFolder, nrFiles + "_" + ".png");
        imageItem.ItemPath = fullFileName;
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
        page.Name = newName;
    }

}