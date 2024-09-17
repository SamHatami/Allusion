using System.Windows;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Interfaces;

public interface IClipboardService
{
    public Task<BitmapImage?[]> GetPastedBitmaps();

    public Task<BitmapImage?[]>? GetDroppedOnCanvasBitmaps(IDataObject droppedObject);
}