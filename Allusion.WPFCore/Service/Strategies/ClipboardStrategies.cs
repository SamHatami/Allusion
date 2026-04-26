using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Extensions;
using System.Threading;
using System.IO;

namespace Allusion.WPFCore.Service.Strategies;

public interface IClipboardDataStrategy
{
    bool CanHandle(IDataObject dataObject);
    Task<BitmapImage?[]> ExtractBitmapsAsync(IDataObject dataObject, CancellationToken cancellationToken = default);
}

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
        return bitmap != null ? [bitmap] : [];
    }
}

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

public class FileDropStrategy : IClipboardDataStrategy
{
    private readonly IBitmapService _bitmapService;

    public FileDropStrategy(IBitmapService bitmapService)
    {
        _bitmapService = bitmapService;
    }

    public bool CanHandle(IDataObject dataObject)
    {
        return dataObject.GetDataPresent(DataFormats.FileDrop);
    }

    public Task<BitmapImage?[]> ExtractBitmapsAsync(IDataObject dataObject, CancellationToken cancellationToken = default)
    {
        var files = dataObject.GetData(DataFormats.FileDrop, true) as string[];
        if (files == null) return Task.FromResult<BitmapImage?[]>([]);

        var bitmaps = files
            .Where(file => File.Exists(file) && SupportedImageFormats.IsSupportedFile(file))
            .Select(file => _bitmapService.GetFromUri(file))
            .Where(bitmap => bitmap != null)
            .ToArray();

        return Task.FromResult<BitmapImage?[]>(bitmaps);
    }
}
