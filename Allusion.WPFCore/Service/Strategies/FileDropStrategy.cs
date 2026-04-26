using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Service.Strategies;

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
        if (files is null) return Task.FromResult<BitmapImage?[]>([]);

        var bitmaps = files
            .Where(file => File.Exists(file) && SupportedImageFormats.IsSupportedFile(file))
            .Select(file => _bitmapService.GetFromUri(file))
            .Where(bitmap => bitmap is not null)
            .ToArray();

        return Task.FromResult<BitmapImage?[]>(bitmaps);
    }
}
