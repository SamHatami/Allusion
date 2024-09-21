using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using System.Windows;
using System.Windows.Media.Imaging;

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

    private async  Task GetPastedImageItems()
    {
        var bitmaps = await _clipboardService.GetPastedBitmaps();

        CreateAndPublishItems(bitmaps);

    }

    public async Task
        GetDroppedImageItems(IDataObject dataobject, Point dropPoint) //Perhaps should go into clipboardservice, unclear
    {
        var bitmaps = await _clipboardService.GetDroppedOnCanvasBitmaps(dataobject);

        CreateAndPublishItems(bitmaps);
    }

    private void CreateAndPublishItems(BitmapImage?[] bitmaps)
    {
        List<ImageItem> items = [];

        var imageItems = bitmaps.Select(bitmap => CreateImageItemFromBitmapImages(bitmap)).ToArray();

        _events.PublishOnUIThreadAsync(new NewImageItemsEvent(imageItems));
    }

    public ImageItem CreateImageItemFromBitmapImages(BitmapImage bitmap, Point dropPoint = default)
    {
        double insertPosX, insertPosY = 0;

        if (dropPoint != default)
        {
            insertPosX = dropPoint.X; //this doesnt seem to be the real droppoint?
            insertPosY = dropPoint.Y;
        }
        else
        {
            insertPosX = new Random().NextDouble() * 50 + 10;
            insertPosY = new Random().NextDouble() * 50 + 10;
        }

        //var path = BitmapUtils.GetUrl(bitmap);

        var item = new ImageItem(insertPosX, insertPosY, 1);
        item.SetSourceImage(bitmap);
        return item;
    }

    public async Task HandleAsync(DragDropEvent message, CancellationToken cancellationToken)
    {
        var dropPoint = message.DropPosition; //Do something useful with this.
        await GetDroppedImageItems(message.DataObject, dropPoint); //published from CanvasBehavior
    }

    public async Task HandleAsync(PasteOnCanvasEvent message, CancellationToken cancellationToken)
    {
       await GetPastedImageItems(); //published from MainViewModel

    }
}