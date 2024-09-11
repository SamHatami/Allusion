using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Service;

namespace Allusion.WPFCore.Handlers;

public class ArtBoardHandler
{
    private ArtBoard? _currentArtBoard;
    private ClipboardService _clipboardService = new();
    private BitmapService _bitmapService = new();
    private ImageItemService _imageItemService = new();

  
    public void GetNewImageItems(int pageNr)
    {
        var dataObject = Clipboard.GetDataObject();
        
        var pastedImages = _clipboardService.GetPastedBitmaps();

        ImageItemService.RetrieveNewImages();
    }
    public void AddImageToBoard()
    {
        var nrOfFils = Directory.GetFiles(_currentArtBoard.FullPath).Length;
    }

    public ArtBoard OpenArtBoard(string fullPath)
    {
        try
        {
            _currentArtBoard = ArtBoard.Read(Path.GetDirectoryName(fullPath));

            foreach(var imageItem in _currentArtBoard.Images)
                imageItem.LoadItemSource();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return _currentArtBoard;
    }

    public async Task SaveImageOnArtBoard(ImageItem[] imageItems)
    {
        _currentArtBoard.Images = imageItems;
        await Task.Run(() => ArtBoard.Save(_currentArtBoard, _currentArtBoard.FullPath));
    }

    private static void SaveBitmapToFile(BitmapSource bitmap)
    {
        using (var fileStream = new FileStream(@"C:\Temp\" + "1.png", FileMode.Create))
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(fileStream);
        }
    }

    private void SaveToGlobalArtBoardList()
    {
        //TODO: 
    }
}