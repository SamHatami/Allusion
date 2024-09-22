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

    private async  Task GetPastedImageItems(Size scaleToSize)
    {
        var bitmaps = await _clipboardService.GetPastedBitmaps();

        CreateAndPublishItems(bitmaps, scaleToSize);

    }

    public async Task GetDroppedImageItems(IDataObject dataobject, Point dropPoint, Size scaleToSize) //Perhaps should go into clipboardservice, unclear
    {
        var bitmaps = await _clipboardService.GetDroppedOnCanvasBitmaps(dataobject);

        CreateAndPublishItems(bitmaps, scaleToSize, dropPoint);
    }

    private void CreateAndPublishItems(BitmapImage?[] bitmaps, Size scaleToSize, Point dropPoint = default)
    {
        List<ImageItem> items = [];

        var imageItems = bitmaps.Select(bitmap => CreateImageItemFromBitmapImages(bitmap,scaleToSize, dropPoint)).ToArray();

        _events.PublishOnUIThreadAsync(new NewImageItemsEvent(imageItems));
    }

    public ImageItem CreateImageItemFromBitmapImages(BitmapImage bitmap,Size scaleToSize, Point dropPoint = default)
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

        var scale = GetScale(scaleToSize.Height, bitmap.PixelHeight);
        var item = new ImageItem(insertPosX, insertPosY, scale);
        item.SetSourceImage(bitmap);
        return item;
    }

    public async Task HandleAsync(DragDropEvent message, CancellationToken cancellationToken)
    {
        var dropPoint = message.DropPosition; //Do something useful with this.
        await GetDroppedImageItems(message.DataObject, dropPoint, message.CurrentWindowSize); //published from CanvasBehavior
    }

    public async Task HandleAsync(PasteOnCanvasEvent message, CancellationToken cancellationToken)
    {
       await GetPastedImageItems(message.CurrentWindowSize); //published from MainViewModel

    }

    private double GetScale(double windowHeight, double pixHeight)
    {
        //If size ratio would be larger than 1, then set scale to be that which corresponds getting the ratio down to about 40%.
        //The size ratio is how much the bitmap would be taking up on the current window.
        var sizeRatio = pixHeight / windowHeight;
        var setHeight = 0.4 * windowHeight;
        var heightRatio = setHeight / pixHeight;

        return (sizeRatio) > 1 ? 1 - (1 - heightRatio) : 1;
    }
}