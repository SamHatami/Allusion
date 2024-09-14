using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Extensions;
using Allusion.WPFCore.Service;
using Caliburn.Micro;

namespace Allusion.WPFCore.Handlers;

public class ArtBoardHandler
{
    public ArtBoard? CurrentArtBoard { get; private set; }
    public AllusionConfiguration CurrentConfiguration { get; private set; }
    private readonly IEventAggregator _events;
    private BitmapService _bitmapService = new();
    private ImageItemService _imageItemService = new();
    private ClipboardService _clipboardService = new();

    public ArtBoardHandler(IEventAggregator events)
    {
        _events = events;
        CurrentConfiguration = AllusionConfiguration.Read();
        CreateNewArtBoard();
    }

    public async Task<ImageItem[]> GetPastedImageItems(int pageNr)
    {
        var bitmaps = await _clipboardService.GetPastedBitmaps();

        List<ImageItem> items = [];
        foreach (var bitmap in bitmaps)
        {
            items.Add(_imageItemService.CreateImageItemFromBitmapImages(bitmap));
        }

        return items.ToArray();
    }


    public void AddImageToBoard()
    {
        var nrOfFils = Directory.GetFiles(CurrentArtBoard.ImageFolder).Length;
    }

    public ArtBoard OpenArtBoard(string fullPath ="")
    {
        try
        {
            var path = string.IsNullOrEmpty(fullPath) ? CurrentArtBoard.ImageFolder : fullPath;

            CurrentArtBoard = ArtBoard.Read(Path.GetDirectoryName(path));

            if (CurrentArtBoard == null) return null; //Do not do this
            {
                foreach (var imageItem in CurrentArtBoard.Images)
                    imageItem.LoadItemSource();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return CurrentArtBoard;
    }

    public void CreateNewArtBoard(string name = "UntitledArtBoard")
    {
        var artboardPath = Path.Combine(CurrentConfiguration.GlobalFolder,name);
        CurrentArtBoard = new ArtBoard(name, artboardPath);
        ArtBoard.Save(CurrentArtBoard);
    }
    public async Task SaveImageOnArtBoard(ImageItem[] imageItems)
    {
        foreach (var imageItem in imageItems)
        {
            if(imageItem.SourceImage.IsEqual(imageItem.SourceImage)) continue;

            CurrentArtBoard.Images.Add(imageItem);
        }
       

        await Task.Run(() => ArtBoard.Save(CurrentArtBoard));
    }



    private void SaveToGlobalArtBoardList()
    {
        //TODO:
    }

    public string[] GetAllArtBoardFolders()
    {
        return Directory.GetDirectories(CurrentConfiguration.GlobalFolder);
    }

    public async Task DroppedNewObjects(IDataObject dataobject)
    {
        var bitmaps =await _clipboardService.GetDroppedOnCanvasBitmaps(dataobject);
        List<ImageItem> images = new List<ImageItem>();
        foreach (var bitmap in bitmaps)
        {
            images.Add(_imageItemService.CreateImageItemFromBitmapImages(bitmap));
        }

        _ = _events.PublishOnBackgroundThreadAsync(new NewImageDropsEvent(images.ToArray()));
        
    }
}