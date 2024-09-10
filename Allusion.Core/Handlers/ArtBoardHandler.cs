using Allusion.Core.Artboard;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Allusion.Core.Handlers;

public class ArtBoardHandler
{
    private ArtBoard? _currentArtBoard;

    public ArtBoardHandler()
    {
    }

    public BitmapSource? GetPastedBitmap()
    {
        var pastedBitmap = Clipboard.GetImage();

        if (pastedBitmap is null) return null;

        SaveBitmapToFile(pastedBitmap);

        return pastedBitmap;
    }

    public ArtBoard OpenProjectFile(string fullPath)
    {
        _currentArtBoard = ArtBoard.Read(Path.GetDirectoryName(fullPath));

        return _currentArtBoard;
    }

    public async Task SaveToProject(ImageItem[] images)
    {
        _currentArtBoard.Images = images;
        await Task.Run(() => ArtBoard.Save(_currentArtBoard, _currentArtBoard.FullPath));
    }

    private void SaveBitmapToFile(BitmapSource bitmap)
    {
        using (var fileStream = new FileStream(@"C:\Temp\" + "1.png", FileMode.Create))
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(fileStream);
        }
    }
}