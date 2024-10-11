using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Allusion.Adorners;
using Allusion.ViewModels;
using Allusion.Views;
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
    private ImageViewModel? _imageViewModel;
    private double _originalTop;
    private double _originalLeft;
    private DragDropEffects dropResult;
    private bool _dropped;
    private DragDropEffects dropEffect;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (!(VisualTreeHelper.GetParent(AssociatedObject) is ContentPresenter contentPresenter)) return;

        _contentPresenter =
            contentPresenter; //The AssociatedObject cant be sized or get the canvas position since its inside a contentcontrol

        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
        _mainCanvas = GetMainCanvas(AssociatedObject);
        if (AssociatedObject is Grid gridContainer && gridContainer.Children[1] is Border border)
            _imageViewModel = border.DataContext as ImageViewModel;

        _dropped = _imageViewModel.Dropped;
        _events = IoC.Get<IEventAggregator>();
        _events.SubscribeOnUIThread(this); //Listen to FileDropEvent to if the imageviewmodel is the same as the one here, otherwise
    }

    private void OnDropped(object sender, DragEventArgs e)
    {

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

        _originalLeft = Canvas.GetLeft(_contentPresenter);
        _originalTop = Canvas.GetTop(_contentPresenter);

        AssociatedObject.CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        //move the icon aswell
        if (AssociatedObject.IsMouseCaptured)
        {
            var newLeft = e.GetPosition(_mainCanvas).X + _relativePosition.X;
            Canvas.SetLeft(_contentPresenter, newLeft < 0 ? 0 : newLeft);

            var newTop = e.GetPosition(_mainCanvas).Y + _relativePosition.Y;
            if (newTop < 0 && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                // If the image goes beyond the top of the canvas, enable drop mode
                EnterDropMode();
                Canvas.SetTop(_contentPresenter, newTop);
                var data = new DataObject();
                data.SetData("ImageVM", _imageViewModel );
                dropEffect = DragDrop.DoDragDrop(AssociatedObject, data, DragDropEffects.Move );

                if (!_dropped && dropEffect == DragDropEffects.None) //Replace with bool 
                {
                    Canvas.SetLeft(_contentPresenter, _originalLeft);
                    Canvas.SetTop(_contentPresenter, _originalTop);
                    Mouse.OverrideCursor = null;
                    ExitDropMode();

                }
            }
            else
            {
                // If the image is within bounds, reset to normal behavior
                ExitDropMode();
                Canvas.SetTop(_contentPresenter, newTop);
            }

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
        ExitDropMode();
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
                BlurRadius = 3,
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

    private void EnterDropMode()
    {
        _contentPresenter.Opacity = 0.5; // Make it semi-transparent to indicate it's being dragged outside
    }

    private void ExitDropMode()
    {
        // Reset any visual effects or flags when the image is back within bounds
        _contentPresenter.Opacity = 1.0; // Reset opacity
    }
}