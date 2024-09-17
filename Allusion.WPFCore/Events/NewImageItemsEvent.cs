using Allusion.WPFCore.Board;

namespace Allusion.WPFCore.Events;

public class NewImageItemsEvent
{
    public ImageItem[] Items { get; }

    public NewImageItemsEvent(ImageItem[] items)
    {
        Items = items;
    }
}