using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using System.Diagnostics;

namespace Allusion.WPFCore.Managers;

public class SessionManager : IHandle<ItemRemovedEvent>, IHandle<UndoItemRemovedEvent>
{
    //IItemOwner = Owner
    //IRemovableItem = item that is allowed to be removed
    private Dictionary<IItemOwner, List<IRemovableItem>> itemBin = [];

    public Task HandleAsync(ItemRemovedEvent message, CancellationToken cancellationToken)
    {
        Debug.Assert(message.Item == null, "remove item was null");

        if(!itemBin.ContainsKey(message.Owner))
            itemBin[message.Owner] = new List<IRemovableItem>();
        itemBin[message.Owner].Add(message.Item);

        return Task.CompletedTask;
    }

    public Task HandleAsync(UndoItemRemovedEvent message, CancellationToken cancellationToken)
    {
        if (!itemBin.TryGetValue(message.Owner, out var items) || !items.Any()) return Task.CompletedTask;
        message.Owner.ReAddItem(items.Last());
        items.RemoveAt(items.Count - 1);

        return Task.CompletedTask;
    }
}