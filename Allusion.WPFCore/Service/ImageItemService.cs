using System.Runtime.InteropServices.ComTypes;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Extensions;
using IDataObject = System.Windows.IDataObject;

namespace Allusion.WPFCore.Service;

public class ImageItemService //Note : could perhaps just be static methods inside the ImageItem class....
{
    public ImageItemService()
    {
    }

    public ImageItem CreateImageItemFromDataObject(BitmapSource bitmap, IDataObject dataObject)
    {
        if (dataObject.TryGetUrl(out string url)) ;

        var randomPos = new Random().Next(10, 60);

        return new ImageItem(url, randomPos, randomPos, 1, 0, bitmap);
    }
}