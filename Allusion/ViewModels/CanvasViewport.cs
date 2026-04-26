using System.Windows;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class CanvasViewport : PropertyChangedBase
{
    public const double DefaultZoom = 1.0;
    public const double MinZoom = 0.1;
    public const double MaxZoom = 5.0;

    private double _zoom = DefaultZoom;
    private double _offsetX;
    private double _offsetY;

    public double Zoom
    {
        get => _zoom;
        private set
        {
            if (Math.Abs(_zoom - value) < 0.0001) return;

            _zoom = value;
            NotifyOfPropertyChange(nameof(Zoom));
        }
    }

    public double OffsetX
    {
        get => _offsetX;
        private set
        {
            if (Math.Abs(_offsetX - value) < 0.0001) return;

            _offsetX = value;
            NotifyOfPropertyChange(nameof(OffsetX));
        }
    }

    public double OffsetY
    {
        get => _offsetY;
        private set
        {
            if (Math.Abs(_offsetY - value) < 0.0001) return;

            _offsetY = value;
            NotifyOfPropertyChange(nameof(OffsetY));
        }
    }

    public void PanBy(Vector delta)
    {
        OffsetX += delta.X;
        OffsetY += delta.Y;
    }

    public void ZoomAt(Point viewportPoint, double wheelDelta)
    {
        var zoomFactor = wheelDelta > 0 ? 1.1 : 1 / 1.1;
        var nextZoom = Clamp(Zoom * zoomFactor, MinZoom, MaxZoom);

        if (Math.Abs(nextZoom - Zoom) < 0.0001) return;

        var worldX = (viewportPoint.X - OffsetX) / Zoom;
        var worldY = (viewportPoint.Y - OffsetY) / Zoom;

        Zoom = nextZoom;
        OffsetX = viewportPoint.X - worldX * Zoom;
        OffsetY = viewportPoint.Y - worldY * Zoom;
    }

    public void Reset()
    {
        Zoom = DefaultZoom;
        OffsetX = 0;
        OffsetY = 0;
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        return value > max ? max : value;
    }
}
