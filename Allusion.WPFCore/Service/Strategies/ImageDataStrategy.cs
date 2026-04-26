using System.Windows;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Service.Strategies;

public class ImageDataStrategy : IClipboardDataStrategy
{
    public bool CanHandle(IDataObject dataObject)
    {
        return dataObject.GetDataPresent(DataFormats.Bitmap);
    }

    public Task<BitmapImage?[]> ExtractBitmapsAsync(IDataObject dataObject, CancellationToken cancellationToken = default)
    {
        var bitmap = dataObject.GetData(DataFormats.Bitmap) as BitmapImage;
        return Task.FromResult<BitmapImage?[]>(bitmap is not null ? [bitmap] : []);
    }
}
