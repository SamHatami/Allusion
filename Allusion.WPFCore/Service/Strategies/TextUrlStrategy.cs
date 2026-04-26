using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Extensions;

namespace Allusion.WPFCore.Service.Strategies;

public class TextUrlStrategy : IClipboardDataStrategy
{
    private readonly DataObjectImageExtractor _extractor;

    public TextUrlStrategy(DataObjectImageExtractor extractor)
    {
        _extractor = extractor;
    }

    public bool CanHandle(IDataObject dataObject)
    {
        return dataObject.GetDataPresent(DataFormats.Text) ||
               dataObject.GetDataPresent(DataFormats.Html);
    }

    public async Task<BitmapImage?[]> ExtractBitmapsAsync(IDataObject dataObject, CancellationToken cancellationToken = default)
    {
        var bitmap = await _extractor.GetWebBitmapAsync(dataObject, cancellationToken);
        return bitmap is not null ? [bitmap] : [];
    }
}
