using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Allusion.Adorners;
using Caliburn.Micro;
using FontAwesome.Sharp;
using Microsoft.Xaml.Behaviors;

namespace Allusion.Behaviors;

public class ImageBehavior : Behavior<UIElement>
{
    private Canvas _mainCanvas;
    private Point _relativePosition;
    private ContentPresenter _contentPresenter;
    private IEventAggregator _events;
    private Image _dragIcon;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (!(VisualTreeHelper.GetParent(AssociatedObject) is ContentPresenter contentPresenter)) return;

        _contentPresenter = contentPresenter; //The AssociatedObject cant be sized or get the canvas position since its inside a contentcontrol

        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
        
        _mainCanvas = GetMainCanvas(AssociatedObject);
        _events = IoC.Get<IEventAggregator>();
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
        if (e.ClickCount == 2)
        {
            //_events.PublishOnBackgroundThreadAsync(new FocusImageEvent())
        }
        //Disable cursor icon
        //enable move icon
        CreateDragIcon(e.GetPosition(_mainCanvas));
        AssociatedObject.Focus();
        Mouse.OverrideCursor = Cursors.None;
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
            var newLeft = e.GetPosition(_mainCanvas).X + _relativePosition.X;
            Canvas.SetLeft(_contentPresenter, newLeft < 0 ? 0:newLeft);

            var newRight = e.GetPosition(_mainCanvas).Y + _relativePosition.Y;
            Canvas.SetTop(_contentPresenter, newRight <0 ? 0:newRight);

            MoveDragIcon(e.GetPosition(_mainCanvas));
        }

        e.Handled = true;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!AssociatedObject.IsMouseCaptured) return;
        Mouse.OverrideCursor = null;
        AssociatedObject.ReleaseMouseCapture();

        RemoveDragIcon();
    }

    private Canvas GetMainCanvas(DependencyObject? element)
    {
        while (element != null)
        {
            if (element is Canvas { Name: "ImageCanvas" } canvas) return canvas;
            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }

    private void CreateDragIcon(Point position)
    {
        _dragIcon = new IconImage()
        {
            Icon = IconChar.UpDownLeftRight, // Example using FontAwesome.Sharp icon
            Width = 24,
            Height = 24,
            Foreground = new SolidColorBrush(Colors.WhiteSmoke),
            Effect = new DropShadowEffect()
            {
                BlurRadius=3,
                ShadowDepth = 0,
                Direction = 0
            }
        };

        Canvas.SetLeft(_dragIcon, position.X);
        Canvas.SetTop(_dragIcon, position.Y);
      

        // Add icon to canvas
        _mainCanvas.Children.Add(_dragIcon);
    }

    // Move the drag icon as the mouse moves
    private void MoveDragIcon(Point position)
    {
        if (_dragIcon != null)
        {
            Canvas.SetLeft(_dragIcon, position.X);
            Canvas.SetTop(_dragIcon, position.Y);


        }
    }

    // Remove the drag icon when resizing is complete
    private void RemoveDragIcon()
    {
        if (_dragIcon != null)
        {
            _mainCanvas.Children.Remove(_dragIcon);
            _dragIcon = null;
        }
    }
}