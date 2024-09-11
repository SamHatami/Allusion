using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Extensions;

namespace Allusion.WPFCore.Service
{
    public class ClipboardService
    {
        public BitmapSource[]? GetPastedBitmaps()  //Make this static that both mainview or canvas can talk to directly.
        {
            var pastedFromWeb = System.Windows.Clipboard.GetImage();
            
            return pastedFromWeb is null ? null : new[] { pastedFromWeb };
        }

        public BitmapSource[]? GetDroppedBitmaps(IDataObject droppedObject)
        {
            //Try getting bitmap if dropped object was from browser
            var getbiBitmapAsync = Task.Run(droppedObject.GetBitmapAsync);

            getbiBitmapAsync.Wait();

            if (getbiBitmapAsync.Result is not null) return new[] { getbiBitmapAsync.Result };

            List<BitmapSource> bitmaps = [];

            if (droppedObject.GetDataPresent(DataFormats.FileDrop))
            {
                var files = droppedObject.GetData(DataFormats.FileDrop, true) as string[];
                bitmaps = BitmapService.GetFromUri(files).ToList();
            }

            return bitmaps.ToArray();
        }
    }
}