using Allusion.WPFCore.Extensions;
using Allusion.WPFCore.Interfaces;
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

    public ClipboardService(IEventAggregator events, IBitmapService bitmapService)
    {
        _events = events;
        _bitmapService = bitmapService;
        _dataObjectImageExtractor = new DataObjectImageExtractor(_bitmapService);
    }

    public async Task<BitmapImage?[]> GetPastedBitmaps()
    {
        List<BitmapImage?> bitmapImages = new();
        IDataObject? pastedUriObject = null;

        // Access the clipboard on the UI thread
        if (Clipboard.ContainsText()) // When pasting SourceImage Uri from web
        {
            pastedUriObject = Clipboard.GetDataObject();
            var bitmapSources = await _dataObjectImageExtractor.GetWebBitmapAsync(pastedUriObject)
                                    ?? _bitmapService.GetFromUri(
                                        _dataObjectImageExtractor.GetLocalFileUrl(pastedUriObject));

                bitmapImages.Add(bitmapSources.ConvertToBitmapImage());
        }
        else if (Clipboard.ContainsImage()) // Usually copy/paste single from web or snapshot
        {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    bitmapImages.Add(GetImage());
                });
        
        }
        else if (Clipboard.ContainsFileDropList()) // Usually copy/paste single or multi from system
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                bitmapImages.AddRange(GetImagesFromClipboard());

            });
        }

        // If a web bitmap needs to be fetched asynchronously

        return bitmapImages.ToArray();
    }

    private BitmapImage GetImage()
    {
        var pasteFromWeb = Clipboard.GetImage();
        var bitmapImage = pasteFromWeb.ConvertToBitmapImage();

        return bitmapImage;
    }

    public async Task<BitmapImage?[]>?
        GetDroppedOnCanvasBitmaps(IDataObject droppedObject) //should not handle SourceImage items
    {
        //TODO: Convert to strategy-pattern
        List<BitmapImage> bitmapImages = [];
        //Try getting bitmap if dropped object was from browser
        var droppedWebBitmap = await _dataObjectImageExtractor.GetWebBitmapAsync(droppedObject).ConfigureAwait(false);

        if (droppedWebBitmap is not null)
            bitmapImages.Add(droppedWebBitmap);

        if (!droppedObject.GetDataPresent(DataFormats.FileDrop)) return bitmapImages.ToArray();

        var files = droppedObject.GetData(DataFormats.FileDrop, true) as string[];

        bitmapImages.AddRange(files.Select(file => _bitmapService.GetFromUri(file)));

        return bitmapImages.ToArray();
    }

    //https://weblog.west-wind.com/posts/2020/Sep/16/Retrieving-Images-from-the-Clipboard-and-WPF-Image-Control-Woes
    private List<BitmapImage> GetImagesFromClipboard()
    {
        var images = new List<BitmapImage>();

        if (Clipboard.ContainsFileDropList())
        {
            var fileDropList = Clipboard.GetFileDropList();
            foreach (var file in fileDropList)
                if (IsImageFile(file))
                    try
                    {
                        //UI-Thread

                        var image = new BitmapImage(new Uri(file));
                        images.Add(image);
                    }
                    catch (Exception ex)
                    {
                        // Handle or log the exception as needed
                        StaticLogger.Error($"Error loading SourceImage {file}: {ex.Message}", true, ex.Message);
                    }
        }

        return images;
    }

    private static bool IsImageFile(string filePath)
    {
        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tga" };
        return imageExtensions.Contains(Path.GetExtension(filePath).ToLower());
    }
}