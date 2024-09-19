using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Events;

public class SessionEvents
{
}

public class ItemRemovedEvent
{
    public IItemOwner Owner;
    public IRemovableItem Item { get; }

    public ItemRemovedEvent(IRemovableItem item, IItemOwner owner)
    {
        Owner = owner;
        Item = item;
    }
}

public class UndoItemRemovedEvent(IItemOwner owner)
{
    public IItemOwner Owner { get; } = owner;
}