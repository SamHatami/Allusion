using System.Drawing;
using Allusion.WPFCore.Board;
using System.Windows.Media;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class FocusViewModel: Screen
{
    public ImageSource ImageSource { get; }

    public double AspectRatio { get; }

    public FocusViewModel(ImageItem item)
    {
        
        ImageSource = item.SourceImage;
        AspectRatio = item.SourceImage.PixelWidth / item.SourceImage.PixelHeight;
    }

    public Task Close()
    {
       return TryCloseAsync();
    }
}