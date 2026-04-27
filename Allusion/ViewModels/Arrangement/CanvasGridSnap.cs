namespace Allusion.ViewModels.Arrangement;

public static class CanvasGridSnap
{
    public const double Size = 25.0;

    public static double Snap(double value)
    {
        return Math.Round(value / Size, MidpointRounding.AwayFromZero) * Size;
    }

    public static double SnapUp(double value)
    {
        return Math.Ceiling(value / Size) * Size;
    }
}
