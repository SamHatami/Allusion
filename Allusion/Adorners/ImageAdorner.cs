using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Allusion.ViewModels;

namespace Allusion.Adorners;

public class ImageAdorner : Adorner
{
    private VisualCollection _visualChildren;
    private Thumb _topLeft, _topRight, _bottomLeft, _bottomRight;
    private const double THUMB_SIZE = 10;
    private UIElement _adornedElement;

    private bool _isResizing;
    private Point _startPoint;
    private Size _originalSize;
    private double _aspectRatio;

    public ImageAdorner(UIElement adornedElement) : base(adornedElement)
    {
        _adornedElement = adornedElement;
        _visualChildren = new VisualCollection(this);

        BuildAdornerCorner(ref _bottomRight);

        _bottomRight.DragDelta += HandleBottomRight;
    }

    private void BuildAdornerCorner(ref Thumb corner)
    {
        corner = new Thumb
            { Width = THUMB_SIZE, Height = THUMB_SIZE, Background = Brushes.Blue, BorderBrush = Brushes.Blue };
        _visualChildren.Add(corner);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _originalSize = AdornedElement.DesiredSize;
        _aspectRatio = _originalSize.Width / _originalSize.Height;
        var desiredWidth = AdornedElement.DesiredSize.Width;
        var desiredHeight = AdornedElement.DesiredSize.Height;

        _bottomRight.Arrange(new Rect(desiredWidth - THUMB_SIZE, desiredHeight - THUMB_SIZE, THUMB_SIZE, THUMB_SIZE));

        return finalSize;
    }

    private void HandleBottomRight(object sender, DragDeltaEventArgs e)
    {
        var contentControl = (FrameworkElement)AdornedElement;

        var horizontalScaleFactor = 1 + e.HorizontalChange * 0.1 / contentControl.Width;
        var verticalScaleFactor = 1 + e.VerticalChange * 0.1 / contentControl.Height;

        var scaleFactor = Math.Min(horizontalScaleFactor, verticalScaleFactor);

        contentControl.Width = Math.Max(0, contentControl.Width * scaleFactor);
        contentControl.Height = Math.Max(0, contentControl.Width / _aspectRatio);
    }

    protected override Visual GetVisualChild(int index)
    {
        return _visualChildren[index];
    }

    protected override int VisualChildrenCount => _visualChildren.Count;
}