using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Extensions;

namespace Allusion.WPFCore.Service
{
    
    public class ClipboardService
    {
        private BitmapService _bitmapService = new BitmapService();
        private DataObjectHelper _dataObjectHelper = new DataObjectHelper();
        public async Task<BitmapImage[]> GetPastedBitmaps()  //Make this static that both mainview or canvas can talk to directly.
        {
            //TODO: Convert to strategy-pattern
            List<BitmapImage> bitmapImages = new List<BitmapImage>();

            if (Clipboard.ContainsText()) //When pasting SourceImage Uri from web 
            {
                var pastedUriObject = Clipboard.GetDataObject();

                var bitmapSources = await _dataObjectHelper.GetWebBitmapAsync(pastedUriObject);


                if (bitmapSources is null) 
                    
                    bitmapSources = BitmapService.GetFromUri(_dataObjectHelper.GetLocalFileUrl(pastedUriObject));

                bitmapImages.Add(_bitmapService.ToBitmapImage(bitmapSources));
            }

            else if (Clipboard.ContainsImage()) //Usually copy/paste single from web or snapshot
            {
                var pasteFromWeb = Clipboard.GetImage();
                var bitmapImage = _bitmapService.ToBitmapImage(pasteFromWeb);
                bitmapImages.Add(bitmapImage);
            }

            else if (Clipboard.ContainsFileDropList()) //usually copy/paste single or multi from system
            {
                bitmapImages.AddRange(GetImagesFromClipboard());
            }

            return bitmapImages.ToArray();
        }

        public async Task<BitmapImage[]>? GetDroppedOnCanvasBitmaps(IDataObject droppedObject) //should not handle SourceImage items
        {
            //TODO: Convert to strategy-pattern
            List<BitmapImage> bitmapImages = [];
            //Try getting bitmap if dropped object was from browser
            var droppedWebBitmap = await _dataObjectHelper.GetWebBitmapAsync(droppedObject).ConfigureAwait(false);

            if (droppedWebBitmap is not null) 
                bitmapImages.Add(droppedWebBitmap);


            if (droppedObject.GetDataPresent(DataFormats.FileDrop))
            {
                var files = droppedObject.GetData(DataFormats.FileDrop, true) as string[];

                foreach (var file in files)
                {
                    bitmapImages.Add(BitmapService.GetFromUri(file));
                }
            }

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
                {
                    if (IsImageFile(file))
                    {
                        try
                        {
                            var image = new BitmapImage(new Uri(file));
                            images.Add(image);
                        }
                        catch (Exception ex)
                        {
                            // Handle or log the exception as needed
                            MessageBox.Show($"Error loading SourceImage {file}: {ex.Message}");
                        }
                    }
                }
            }

            return images;
        }

        private static bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp",".tif" };
            return imageExtensions.Contains(Path.GetExtension(filePath).ToLower());
        }
    }
}