using System.Windows;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Service.Strategies;

public interface IClipboardDataStrategy
{
    bool CanHandle(IDataObject dataObject);
    Task<BitmapImage?[]> ExtractBitmapsAsync(IDataObject dataObject, CancellationToken cancellationToken = default);
}
