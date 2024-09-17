using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Caliburn.Micro;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Service;

public class ImageItemService : IHandle<DragDropEvent> 
{
    private readonly IEventAggregator _events;
    private readonly ClipboardService _clipboardService;
    private BitmapService _bitmapService = new();

    public ImageItemService(IEventAggregator events, ClipboardService clipboardService)
    {
        _events = events;
        _clipboardService = clipboardService;
        _events.SubscribeOnBackgroundThread(this);
    }

    private async Task GetPastedImageItems()
    {
        var bitmaps = await _clipboardService.GetPastedBitmaps();

        CreateAndPublishItems(bitmaps);
    }

    private void CreateAndPublishItems(BitmapImage?[] bitmaps)
    {
        List<ImageItem> items = [];

        var imageItems = bitmaps.Select(bitmap => CreateImageItemFromBitmapImages(bitmap)).ToArray();

        _events.PublishOnBackgroundThreadAsync(new NewImageItemsEvent(imageItems));
    }

    public async Task GetDroppedImageItems(IDataObject dataobject) //allt detta ska till clipboardservice?
    {
        var bitmaps = await _clipboardService.GetDroppedOnCanvasBitmaps(dataobject);

        CreateAndPublishItems(bitmaps);
    }

    public ImageItem CreateImageItemFromBitmapImages(BitmapImage bitmap)
    {
        var randomPos = new Random().NextDouble() * 50 + 10;

        var path = BitmapService.GetUrl(bitmap);

        var item = new ImageItem(path, randomPos, randomPos, 1);
        item.SetSourceImage(bitmap);
        return item;
    }

    public async Task HandleAsync(DragDropEvent message, CancellationToken cancellationToken)
    {
        await GetDroppedImageItems(message.DataObject);
    }
}