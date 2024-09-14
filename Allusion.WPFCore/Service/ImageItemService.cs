using System.Runtime.InteropServices.ComTypes;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Extensions;
using IDataObject = System.Windows.IDataObject;

namespace Allusion.WPFCore.Service;

public class ImageItemService //Note : could perhaps just be static methods inside the ImageItem class....
{
    private BitmapService _bitmapService = new BitmapService();
    public ImageItemService()
    {
    }

    public ImageItem CreateImageItemFromBitmapImages(BitmapImage bitmap)
    {
        var randomPos = new Random().NextDouble()*50+10;

        var path = BitmapService.GetUrl(bitmap);
        
        return new ImageItem(path,randomPos, randomPos, 1, 0, bitmap);
    }
}