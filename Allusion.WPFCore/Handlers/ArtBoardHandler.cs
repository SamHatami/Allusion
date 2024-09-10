using Allusion.WPFCore.Artboard;
using Allusion.WPFCore.Extensions;
using Allusion.WPFCore.Helpers;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using static System.Net.WebRequestMethods;

namespace Allusion.WPFCore.Handlers;

public class ArtBoardHandler
{
    private ArtBoard? _currentArtBoard;

    public ArtBoardHandler()
    {
    }

    public static BitmapSource[]? GetPastedBitmaps()  //Make this static that both mainview or canvas can talk to directly.
    {
        var pastedFromWeb = Clipboard.GetImage();

        if (pastedFromWeb != null) return new[] { pastedFromWeb };


        var droppedObject = Clipboard.GetDataObject();

        if (droppedObject == null) return null;

        List<BitmapSource> bitmaps = new List<BitmapSource>();
                
        if(droppedObject.GetDataPresent(DataFormats.FileDrop))
        {
            var files = droppedObject.GetData(DataFormats.FileDrop, true) as string[];
            bitmaps = BitmapHelper.GetImagesFromUri(files).ToList();
        }

        foreach (var bitmap in bitmaps)
            SaveBitmapToFile(bitmap);

        return bitmaps.ToArray();
    }

    public void AddImageToBoard()
    {
        var nrOfFils = Directory.GetFiles(_currentArtBoard.FullPath).Length;
    }

    public ArtBoard OpenProjectFile(string fullPath)
    {
        _currentArtBoard = ArtBoard.Read(Path.GetDirectoryName(fullPath));

        return _currentArtBoard;
    }

    public async Task SaveImageOnArtBoard(ImageItem[] images)
    {
        _currentArtBoard.Images = images;
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
}