using Allusion.ViewModels;

namespace Allusion.Events;

public class SelectionEvent(ImageViewModel[] images, SelectionType type = SelectionType.Single)
{
    public ImageViewModel[]? Images { get; } = images;
    public SelectionType Type { get; }
}

public enum SelectionType
{
    Single,
    Multi,
    DeSelect
}