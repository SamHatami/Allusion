namespace Allusion.ViewModels.Arrangement;

public enum ArrangeScaleMode
{
    KeepCurrent,
    AverageHeight,
    SmallestHeight
}

public sealed record ArrangeImageLayoutItem(double Width, double Height, double Scale, double ExtraHeight = 0);

public sealed record ArrangeImageLayoutResult(double X, double Y, double Scale);

public sealed record ArrangeImageLayoutOptions
{
    public double Margin { get; init; } = 24;
    public ArrangeScaleMode ScaleMode { get; init; } = ArrangeScaleMode.KeepCurrent;
    public int Columns { get; init; } = 0;
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

        var columnCount = ResolveColumnCount(items.Count, options.Columns);
        var results = new List<ArrangeImageLayoutResult>(items.Count);
        var xCursor = 0.0;
        var yCursor = 0.0;
        var rowHeight = 0.0;

        for (var i = 0; i < items.Count; i++)
        {
            var startsNewRow = i > 0 && i % columnCount == 0;
            if (startsNewRow)
            {
                xCursor = 0;
                yCursor += rowHeight + margin;
                rowHeight = 0;
            }

            var size = arrangedSizes[i];
            results.Add(new ArrangeImageLayoutResult(
                CanvasGridSnap.Snap(xCursor),
                CanvasGridSnap.SnapUp(yCursor),
                size.Scale));

            xCursor += size.Width + margin;
            rowHeight = Math.Max(rowHeight, size.Height + size.ExtraHeight);
        }

        return results;
    }

    private static int ResolveColumnCount(int itemCount, int requestedColumns)
    {
        if (requestedColumns > 0)
            return requestedColumns;

        return Math.Max(1, (int)Math.Ceiling(Math.Sqrt(itemCount)));
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
        var width = Math.Max(0, item.Width);
        var height = Math.Max(0, item.Height);
        var extraHeight = Math.Max(0, item.ExtraHeight);
        if (targetHeight is null || height <= 0)
            return new ArrangeImageLayoutItem(width, height, item.Scale, extraHeight);

        var scaleFactor = targetHeight.Value / height;
        return new ArrangeImageLayoutItem(
            width * scaleFactor,
            targetHeight.Value,
            item.Scale * scaleFactor,
            extraHeight);
    }
}
