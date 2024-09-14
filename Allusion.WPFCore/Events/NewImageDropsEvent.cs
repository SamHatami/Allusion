using System.Windows.Controls.Primitives;
using Allusion.WPFCore.Board;

namespace Allusion.WPFCore.Events;

public class NewImageDropsEvent
{
    public ImageItem[] DroppedItems { get; set; }

    public NewImageDropsEvent(ImageItem[] droppedItems)
    {
        DroppedItems = droppedItems;
    }
}



