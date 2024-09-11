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

    public void CreateImageItemFromDataObject(BitmapSource bitmap, IDataObject dataObject)
    {
        if(dataObject.TryGetUrl(out string url));
        ImageItem item = new ImageItem(url, 30, 30, 1, 0, bitmap);
    }

    public static void RetrieveNewImages()
    {
        throw new NotImplementedException();
    }
}