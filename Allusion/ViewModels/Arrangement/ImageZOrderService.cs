namespace Allusion.ViewModels.Arrangement;

public enum ZOrderOperation
{
    BringToFront,
    SendToBack,
    BringForward,
    SendBackward
}

public static class ImageZOrderService
{
    public static IReadOnlyList<T> Reorder<T>(
        IReadOnlyList<T> stackBottomToTop,
        IReadOnlyCollection<T> selected,
        ZOrderOperation operation)
    {
        if (stackBottomToTop.Count == 0 || selected.Count == 0)
            return stackBottomToTop;

        var moving = new HashSet<T>(selected);
        var order = stackBottomToTop.ToList();

        if (!order.Any(moving.Contains))
            return order;

        return operation switch
        {
            ZOrderOperation.BringToFront => MoveToTop(order, moving),
            ZOrderOperation.SendToBack => MoveToBottom(order, moving),
            ZOrderOperation.BringForward => Step(order, moving, 1),
            ZOrderOperation.SendBackward => Step(order, moving, -1),
            _ => order
        };
    }

    private static IReadOnlyList<T> MoveToTop<T>(List<T> order, HashSet<T> moving)
    {
        var result = order.Where(item => !moving.Contains(item)).ToList();
        result.AddRange(order.Where(moving.Contains));
        return result;
    }

    private static IReadOnlyList<T> MoveToBottom<T>(List<T> order, HashSet<T> moving)
    {
        var result = order.Where(moving.Contains).ToList();
        result.AddRange(order.Where(item => !moving.Contains(item)));
        return result;
    }

    private static IReadOnlyList<T> Step<T>(List<T> order, HashSet<T> moving, int direction)
    {
        var result = new List<T>(order);

        // Walk against the direction of travel so a moving item never swaps into
        // a slot that another moving item has just vacated.
        for (var i = direction > 0 ? result.Count - 1 : 0; i >= 0 && i < result.Count; i -= direction)
        {
            if (!moving.Contains(result[i])) continue;

            var target = i + direction;
            if (target < 0 || target >= result.Count) continue;
            if (moving.Contains(result[target])) continue;

            (result[i], result[target]) = (result[target], result[i]);
        }

        return result;
    }
}
