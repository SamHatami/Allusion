using System.Windows;
using System.Windows.Media.Imaging;
using System.Threading;

namespace Allusion.WPFCore.Interfaces;

public interface IClipboardService
{
    public Task<BitmapImage?[]> GetPastedBitmaps();

    public Task<BitmapImage?[]> GetDroppedOnCanvasBitmaps(IDataObject droppedObject, CancellationToken cancellationToken = default);
}