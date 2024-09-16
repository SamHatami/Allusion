using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using Caliburn.Micro;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Allusion.WPFCore.Handlers;

public class RefBoardHandler : IReferenceBoardHandler
{
    public ReferenceBoard? CurrentRefBoard { get; private set; }
    public AllusionConfiguration CurrentConfiguration { get; private set; }
    private readonly IEventAggregator _events;
    private BitmapService _bitmapService = new();
    private readonly ImageItemService _imageItemService = new();
    private readonly ClipboardService _clipboardService = new();

    public RefBoardHandler(IEventAggregator events, AllusionConfiguration configuration)
    {
        _events = events;
        CurrentConfiguration = configuration;
    }

    public async Task<ImageItem[]> GetPastedImageItems(int pageNr)
    {
        var bitmaps = await _clipboardService.GetPastedBitmaps();

        List<ImageItem> items = [];
        foreach (var bitmap in bitmaps) items.Add(_imageItemService.CreateImageItemFromBitmapImages(bitmap));

        return items.ToArray();
    }

    public void AddImage(ImageItem imageItem, BoardPage page)
    {
        var nrFiles = Directory.GetFiles(CurrentRefBoard.BaseFolder).Length;

        if (page.ImageItems.Contains(imageItem)) return;

        imageItem.MemberOfPage = page.BoardId;
        page.ImageItems.Add(imageItem);
        var fullFileName = Path.Combine(CurrentRefBoard.BaseFolder, nrFiles + 1 + "_" + ".png");
        imageItem.ItemPath = fullFileName;
    }

    public void RemoveImage(ImageItem imageItem)
    {
        Debug.Assert(CurrentRefBoard != null, nameof(CurrentRefBoard) + " != null");

        var page = CurrentRefBoard.Pages.Single(p => p.BoardId == imageItem.MemberOfPage);
        if (page.ImageItems.Contains(imageItem)) return; 

        page.ImageItems.Remove(imageItem);
    }

    public void OpenRefBoard(string fullPath = "")
    {
        try
        {
            var path = string.IsNullOrEmpty(fullPath) ? CurrentRefBoard.BaseFolder : fullPath;

            if (!File.Exists(path)) return; //använd catchen

            CurrentRefBoard = ReferenceBoard.Read(path);

            _events.PublishOnBackgroundThreadAsync(new OpenRefBoardEvent(CurrentRefBoard));
        }
        catch (Exception e)

        {
            Trace.WriteLine(e);
            throw;
        }
    }

    public void CreateNewRefBoard(string name = "UntitledRefBoard")
    {
        var RefBoardPath = Path.Combine(CurrentConfiguration.GlobalFolder, name);
        CurrentRefBoard = new ReferenceBoard(name, RefBoardPath);
        ReferenceBoard.Save(CurrentRefBoard);
    }

    public async Task<bool> SaveRefBoard(ImageItem[] imageItems)
    {
        bool saved;

        try
        {
            saved = await Task.Run(() => ReferenceBoard.Save(CurrentRefBoard));
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            throw;
        }

        return saved;
    }

    private string[] GetAllRefBoardFolders()
    {
        return Directory.GetDirectories(CurrentConfiguration.GlobalFolder);
    }

    public RefBoardInfo[] GetAllRefBoardInfos()
    {
        var folders = GetAllRefBoardFolders();

        List<RefBoardInfo> infos = new();

        foreach (var folder in folders)
        {
            var name = Path.GetFileName(folder);
            var creationTime = Directory.GetCreationTime(folder);
            var changeTime = Directory.GetLastWriteTime(folder);
            var lastOpenTime = Directory.GetLastAccessTime(folder);
            var filePath = Path.Combine(CurrentConfiguration.GlobalFolder, name);

            infos.Add(new RefBoardInfo(name, folder, filePath)
            {
                CreatedDate = creationTime,
                LastWrite = changeTime,
                LastAccess = lastOpenTime
            });
        }

        return infos.ToArray();
    }

    public async Task<ImageItem[]> GetDroppedImageItems(IDataObject dataobject)
    {
        var bitmaps = await _clipboardService.GetDroppedOnCanvasBitmaps(dataobject);

        List<ImageItem> images = new();
        foreach (var bitmap in bitmaps) images.Add(_imageItemService.CreateImageItemFromBitmapImages(bitmap));

        _ = _events.PublishOnBackgroundThreadAsync(new NewImageDropsEvent(images.ToArray()));

        return images.ToArray();
    }

    private void SaveToGlobalRefBoardList()
    {
        //TODO:
    }
}