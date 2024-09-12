using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Extensions;

namespace Allusion.WPFCore.Service
{
    public static class ClipboardService
    {
        public static BitmapImage[]? GetPastedBitmaps()  //Make this static that both mainview or canvas can talk to directly.
        {
            if (Clipboard.ContainsText()) //When pasting image Uri from web 
            {
                var pastedUriObject = Clipboard.GetDataObject();

                var downloadTask = Task.Run(() => pastedUriObject.GetWebBitmapAsync());
                downloadTask.Wait();

                var res = downloadTask.Result;
            }

            if (Clipboard.ContainsImage()) //Usually from web or snapshot
            {
                var pasteFromWeb = Clipboard.GetImage();
            }

            if (Clipboard.ContainsFileDropList()) //usually from system
            {
                return GetImagesFromClipboard().ToArray() ;
            }

            var pastedfromSystem = Clipboard.GetDataObject();

            //var bitmaps = pastedfromSystem.GetBitmapFromLocal();

            return null;
        }

        public static ImageItem[]? GetDroppedBitmaps(IDataObject droppedObject)
        {
            //Try getting bitmap if dropped object was from browser
            var getBitmapAsync = Task.Run(droppedObject.GetWebBitmapAsync);

            getBitmapAsync.Wait();

            if (getBitmapAsync.Result is not null)
            {
                return null ;
            }

            List<ImageItem> imageitems = new List<ImageItem>();

            if (droppedObject.GetDataPresent(DataFormats.FileDrop))
            {
                var files = droppedObject.GetData(DataFormats.FileDrop, true) as string[];

                foreach (var file in files)
                {
                    var bitmap = BitmapService.GetFromUri(file);
                    imageitems.Add(new ImageItem(file, 0, 0, 1, 0, bitmap));
                }
            }

            return imageitems.ToArray();
        }

        private static List<BitmapImage> GetImagesFromClipboard()
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
                            MessageBox.Show($"Error loading image {file}: {ex.Message}");
                        }
                    }
                }
            }

            return images;
        }

        private static bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            return imageExtensions.Contains(Path.GetExtension(filePath).ToLower());
        }
    }
}