
using System.Windows;

namespace Allusion.WPFCore.Events;

public class DragDropEvent
{
    public IDataObject DataObject { get; }
    public Point DropPosition { get; }
    public Size CurrentWindowSize { get; }

    public DragDropEvent(IDataObject dataObject, Point dropPosition, Size currentWindowSize)
    {
        DataObject = dataObject;
        DropPosition = dropPosition;
        CurrentWindowSize = currentWindowSize;
    }
}