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

        _contentPresenter = contentPresenter; //The AssociatedObject cant be sized or get the canvas position since its inside a contentcontrol

        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
        
        _mainCanvas = GetMainCanvas(AssociatedObject);

        
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
        //Disable cursor icon
        //enable move icon
        AssociatedObject.Focus();
        Mouse.OverrideCursor = Cursors.ScrollAll;
        _relativePosition = new Point(
            Canvas.GetLeft(_contentPresenter) - e.GetPosition(_mainCanvas).X,
            Canvas.GetTop(_contentPresenter) - e.GetPosition(_mainCanvas).Y
        );

        AssociatedObject.CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        //move the icon aswell
        if (AssociatedObject.IsMouseCaptured)
        {
            Canvas.SetLeft(_contentPresenter, e.GetPosition(_mainCanvas).X + _relativePosition.X);
            Canvas.SetTop(_contentPresenter, e.GetPosition(_mainCanvas).Y + _relativePosition.Y);
        }

        e.Handled = true;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!AssociatedObject.IsMouseCaptured) return;
        Mouse.OverrideCursor = null;
        AssociatedObject.ReleaseMouseCapture();
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