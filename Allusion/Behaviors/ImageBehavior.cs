using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Allusion.Adorners;
using Microsoft.Xaml.Behaviors;

namespace Allusion.Behaviors;

public class ImageBehavior : Behavior<UIElement>
{
    private Canvas _mainCanvas;
    private Point _relativePosition;
    private ContentPresenter _contentPresenter;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (!(VisualTreeHelper.GetParent(AssociatedObject) is ContentPresenter contentPresenter)) return;

        _contentPresenter = contentPresenter;

        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;

        _mainCanvas = GetMainCanvas(AssociatedObject);

        SetAdorner();
    }

    private void SetAdorner()
    {
        //var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject); //Should perhaps be contentPresenter?
        //if (adornerLayer != null) adornerLayer.Add(new ImageAdorner(AssociatedObject));
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        AssociatedObject.Focus();

        var mousePos = e.GetPosition(_mainCanvas);

        _relativePosition = new Point(
            Canvas.GetLeft(_contentPresenter) - mousePos.X,
            Canvas.GetTop(_contentPresenter) - mousePos.Y
        );

        AssociatedObject.CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (AssociatedObject.IsMouseCaptured)
        {
            var mouseCurrentPosition = e.GetPosition(_mainCanvas);

            MoveSelected(mouseCurrentPosition);
        }

        e.Handled = true;
    }

    private void MoveSelected(Point mousePos)
    {
        var newX = mousePos.X + _relativePosition.X;
        var newY = mousePos.Y + _relativePosition.Y;

        if (double.IsNaN(newX) || double.IsNaN(newY))
            return;

        Canvas.SetLeft(_contentPresenter, newX);
        Canvas.SetTop(_contentPresenter, newY);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject.IsMouseCaptured) AssociatedObject.ReleaseMouseCapture();
    }

    private Canvas GetMainCanvas(DependencyObject? element)
    {
        while (element != null)
        {
            if (element is Canvas { Name: "MainCanvas" } canvas) return canvas;
            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }
}