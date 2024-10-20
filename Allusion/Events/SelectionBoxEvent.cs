using Allusion.ViewModels;

namespace Allusion.Events;

public class SelectionBoxEvent(ImageViewModel[] images)
{
    public ImageViewModel[] Images { get; } = images;
}