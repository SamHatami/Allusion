namespace Allusion.ViewModels.Arrangement;

public enum AlignEdge
{
    Left,
    Right,
    Top,
    Bottom,
    HorizontalCenters,
    VerticalCenters
}

public sealed record ImageAlignItem(double X, double Y, double Width, double Height);

public sealed record ImageAlignResult(double X, double Y);

public static class ImageAlignmentService
{
    public static IReadOnlyList<ImageAlignResult> Align(IReadOnlyList<ImageAlignItem> items, AlignEdge edge)
    {
        if (items.Count < 2)
            return items.Select(item => new ImageAlignResult(item.X, item.Y)).ToArray();

        var left = items.Min(item => item.X);
        var top = items.Min(item => item.Y);
        var right = items.Max(item => item.X + item.Width);
        var bottom = items.Max(item => item.Y + item.Height);
        var centerX = (left + right) / 2;
        var centerY = (top + bottom) / 2;

        return items.Select(item => edge switch
        {
            AlignEdge.Left => new ImageAlignResult(left, item.Y),
            AlignEdge.Right => new ImageAlignResult(right - item.Width, item.Y),
            AlignEdge.Top => new ImageAlignResult(item.X, top),
            AlignEdge.Bottom => new ImageAlignResult(item.X, bottom - item.Height),
            AlignEdge.HorizontalCenters => new ImageAlignResult(centerX - item.Width / 2, item.Y),
            AlignEdge.VerticalCenters => new ImageAlignResult(item.X, centerY - item.Height / 2),
            _ => new ImageAlignResult(item.X, item.Y)
        }).ToArray();
    }
}
