using Allusion.WPFCore.Board;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Service;

public class ImageItemService //Note : could perhaps just be static methods inside the ImageItem class....
{
    private BitmapService _bitmapService = new();

    public ImageItemService()
    {
    }

    public ImageItem CreateImageItemFromBitmapImages(BitmapImage bitmap)
    {
        var randomPos = new Random().NextDouble() * 50 + 10;

        var path = BitmapService.GetUrl(bitmap);

        var item = new ImageItem(path, randomPos, randomPos, 1, 0);
        item.SetSourceImage(bitmap);
        return item;
    }
}