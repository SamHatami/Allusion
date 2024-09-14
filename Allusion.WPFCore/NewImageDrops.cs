using System.Windows.Controls.Primitives;
using Allusion.WPFCore.Board;

namespace Allusion.WPFCore;

public class NewImageDrops
{
    public ImageItem[] DroppedItems { get; set; }

    public NewImageDrops(ImageItem[] droppedItems )
    {
        DroppedItems = droppedItems;
    }
}



