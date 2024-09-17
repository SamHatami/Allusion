
using System.Windows;

namespace Allusion.WPFCore.Events;

public class DragDropEvent
{
    public IDataObject DataObject { get; }

    public DragDropEvent(IDataObject dataObject)
    {
        DataObject = dataObject;
    }
}