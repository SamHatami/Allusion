
using System.Windows;

namespace Allusion.WPFCore.Events;

public class DragDropEvent
{
    public IDataObject DataObject { get; }
    public Point DropPosition { get; }

    public DragDropEvent(IDataObject dataObject, Point dropPosition)
    {
        DataObject = dataObject;
        DropPosition = dropPosition;
    }
}