using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Caliburn.Micro;
using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Utilities;

namespace Allusion.WPFCore.Service;

public class ImageItemService : IHandle<DragDropEvent>, IHandle<PasteOnCanvasEvent>
{
    private readonly IEventAggregator _events;
    private readonly IClipboardService _clipboardService;

    public ImageItemService(IEventAggregator events, IClipboardService clipboardService)
    {
        _events = events;
        _clipboardService = clipboardService;
        _events.SubscribeOnBackgroundThread(this);
    }

    private async Task GetPastedImageItems()
    {
        var bitmaps = await _clipboardService.GetPastedBitmaps();

        await CreateAndPublishItems(bitmaps);
    }

    private async Task CreateAndPublishItems(BitmapImage?[] bitmaps)
    {
        List<ImageItem> items = [];

        var imageItems = bitmaps.Select(bitmap => CreateImageItemFromBitmapImages(bitmap)).ToArray();

        await _events.PublishOnBackgroundThreadAsync(new NewImageItemsEvent(imageItems));
    }

    public async Task GetDroppedImageItems(IDataObject dataobject) //allt detta ska till clipboardservice?
    {
        var bitmaps = await _clipboardService.GetDroppedOnCanvasBitmaps(dataobject);

        await CreateAndPublishItems(bitmaps);
    }

    public ImageItem CreateImageItemFromBitmapImages(BitmapImage bitmap)
    {
        var randomPos = new Random().NextDouble() * 50 + 10;

        var path = BitmapUtils.GetUrl(bitmap);

        var item = new ImageItem(path, randomPos, randomPos, 1);
        item.SetSourceImage(bitmap);
        return item;
    }

    public async Task HandleAsync(DragDropEvent message, CancellationToken cancellationToken)
    {
        var dropPoint = message.DropPosition; //Do something useful with this.
        await GetDroppedImageItems(message.DataObject); //published from CanvasBehavior
    }

    public async Task HandleAsync(PasteOnCanvasEvent message, CancellationToken cancellationToken)
    {
        await GetPastedImageItems(); //published from MainViewModel
    }
}