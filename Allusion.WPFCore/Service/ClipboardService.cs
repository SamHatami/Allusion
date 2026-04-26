using Allusion.WPFCore.Extensions;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service.Strategies;
using Caliburn.Micro;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Service;

public class ClipboardService : IClipboardService
{
    private readonly IEventAggregator _events;
    private readonly DataObjectImageExtractor _dataObjectImageExtractor;
    private readonly IBitmapService _bitmapService;
    private readonly IEnumerable<IClipboardDataStrategy> _strategies;

    public ClipboardService(IEventAggregator events, IBitmapService bitmapService)
    {
        _events = events;
        _bitmapService = bitmapService;
        _dataObjectImageExtractor = new DataObjectImageExtractor(_bitmapService);

        // Initialize strategies
        _strategies = new IClipboardDataStrategy[]
        {
            new TextUrlStrategy(_dataObjectImageExtractor),
            new ImageDataStrategy(),
            new FileDropStrategy(_bitmapService)
        };
    }

    public async Task<BitmapImage?[]> GetPastedBitmaps()
    {
        var dataObject = Clipboard.GetDataObject();
        if (dataObject == null) return Array.Empty<BitmapImage>();

        // Try each strategy in order
        foreach (var strategy in _strategies)
        {
            if (strategy.CanHandle(dataObject))
            {
                try
                {
                    return await strategy.ExtractBitmapsAsync(dataObject);
                }
                catch (Exception ex)
                {
                    StaticLogger.Error($"Error extracting bitmaps with {strategy.GetType().Name}: {ex.Message}", true, ex.Message);
                    // Continue to next strategy
                }
            }
        }

        return Array.Empty<BitmapImage>();
    }

    public async Task<BitmapImage?[]> GetDroppedOnCanvasBitmaps(IDataObject droppedObject, CancellationToken cancellationToken = default)
    {
        if (droppedObject == null) return Array.Empty<BitmapImage>();

        var allBitmaps = new List<BitmapImage>();

        // Try each strategy in order
        foreach (var strategy in _strategies)
        {
            if (strategy.CanHandle(droppedObject))
            {
                try
                {
                    var bitmaps = await strategy.ExtractBitmapsAsync(droppedObject, cancellationToken);
                    allBitmaps.AddRange(bitmaps.OfType<BitmapImage>());
                }
                catch (Exception ex)
                {
                    StaticLogger.Error($"Error extracting dropped bitmaps with {strategy.GetType().Name}: {ex.Message}", true, ex.Message);
                    // Continue to next strategy
                }
            }
        }

        return allBitmaps.ToArray();
    }
}
