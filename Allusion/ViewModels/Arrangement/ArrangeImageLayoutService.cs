namespace Allusion.ViewModels.Arrangement;

public enum ArrangeScaleMode
{
    KeepCurrent,
    AverageHeight,
    SmallestHeight
}

public sealed record ArrangeImageLayoutItem(double Width, double Height, double Scale);

public sealed record ArrangeImageLayoutResult(double X, double Y, double Scale);

public sealed class ArrangeImageLayoutOptions
{
    public double Margin { get; init; } = 24;
    public ArrangeScaleMode ScaleMode { get; init; } = ArrangeScaleMode.KeepCurrent;
}

public class ArrangeImageLayoutService
{
    public IReadOnlyList<ArrangeImageLayoutResult> Arrange(IReadOnlyList<ArrangeImageLayoutItem> items, ArrangeImageLayoutOptions options)
    {
        if (items.Count == 0) return [];

        var margin = Math.Max(0, options.Margin);
        var targetHeight = GetTargetHeight(items, options.ScaleMode);
        var arrangedSizes = items
            .Select(item => GetArrangedSize(item, targetHeight))
            .ToArray();

        var columnCount = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(items.Count)));
        var results = new List<ArrangeImageLayoutResult>(items.Count);
        var x = 0.0;
        var y = 0.0;
        var rowHeight = 0.0;

        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0 && i % columnCount == 0)
            {
                x = 0;
                y = CanvasGridSnap.SnapUp(y + rowHeight + margin);
                rowHeight = 0;
            }

            var size = arrangedSizes[i];
            results.Add(new ArrangeImageLayoutResult(x, y, size.Scale));

            x = CanvasGridSnap.SnapUp(x + size.Width + margin);
            rowHeight = Math.Max(rowHeight, size.Height);
        }

        return results;
    }

    private static double? GetTargetHeight(IReadOnlyList<ArrangeImageLayoutItem> items, ArrangeScaleMode scaleMode)
    {
        var heights = items
            .Where(item => item.Height > 0)
            .Select(item => item.Height)
            .ToArray();

        if (heights.Length == 0 || scaleMode == ArrangeScaleMode.KeepCurrent)
            return null;

        return scaleMode switch
        {
            ArrangeScaleMode.AverageHeight => heights.Average(),
            ArrangeScaleMode.SmallestHeight => heights.Min(),
            _ => null
        };
    }

    private static ArrangeImageLayoutItem GetArrangedSize(ArrangeImageLayoutItem item, double? targetHeight)
    {
        if (targetHeight is null || item.Height <= 0)
            return item;

        var scaleFactor = targetHeight.Value / item.Height;
        return item with
        {
            Width = item.Width * scaleFactor,
            Height = targetHeight.Value,
            Scale = item.Scale * scaleFactor
        };
    }
}
