using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Allusion.Events;
using Allusion.ViewModels;
using Caliburn.Micro;
using Microsoft.Xaml.Behaviors;

namespace Allusion.Behaviors;

public class CanvasSelectionBoxBehavior : Behavior<UIElement>
{
    private List<ImageViewModel> _images = [];
    private Rectangle? _selectionRectangle;
    private Point _startPoint;
    private Canvas? _mainCanvas;
    private FrameworkElement? _inputSurface;
    private PageViewModel? _page;

    private IEventAggregator? _events;
    private double _signedWidth;
    private double _signedHeight;
    private bool _isSelecting;

    protected override void OnAttached()
    {
        base.OnAttached();

        _events = IoC.Get<IEventAggregator>();

        if (AssociatedObject is Canvas canvas)
        {
            _mainCanvas = canvas;
            _page = _mainCanvas.DataContext as PageViewModel;
            _inputSurface = VisualTreeHelper.GetParent(_mainCanvas) as FrameworkElement ?? _mainCanvas;

            _inputSurface.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            _inputSurface.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            _inputSurface.PreviewMouseMove += OnMouseMove;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (_inputSurface == null) return;

        _inputSurface.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
        _inputSurface.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
        _inputSurface.PreviewMouseMove -= OnMouseMove;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isSelecting && _inputSurface is not null && _inputSurface.IsMouseCaptured)
        {
            CreateSelectionHitTest();

            if(_images.Any())
                _events.PublishOnBackgroundThreadAsync(new SelectionEvent(_images.ToArray(), SelectionType.Multi));

            _inputSurface.ReleaseMouseCapture();
            ResetSelectionBox();
            _isSelecting = false;
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_mainCanvas is null || _inputSurface is null) return;
        if (Keyboard.IsKeyDown(Key.Space)) return;
        if (IsImageHit(e.OriginalSource as DependencyObject)) return;

        _startPoint = e.GetPosition(_mainCanvas);
        _images.Clear();

        _events?.PublishOnUIThreadAsync(new SelectionEvent(null, SelectionType.DeSelect));
        _mainCanvas.Focus();
        _inputSurface.CaptureMouse();
        _isSelecting = true;
        _selectionRectangle = _mainCanvas.FindName("SelectionRectangle") as Rectangle;
        SetSelectionBoxStart();

        e.Handled = false;
    }

    private void CreateSelectionHitTest()
    {
        if (_page is null) return;

        var selectionBounds = GetSelectionBounds();

        foreach (var image in _page.Images)
        {
            var imageBounds = new Rect(image.PosX, image.PosY, image.Width, image.Height);
            if (selectionBounds.IntersectsWith(imageBounds))
                _images.Add(image);
        }
    }

    private Rect GetSelectionBounds()
    {
        var left = _signedWidth < 0 ? _startPoint.X + _signedWidth : _startPoint.X;
        var top = _signedHeight < 0 ? _startPoint.Y + _signedHeight : _startPoint.Y;

        return new Rect(left, top, Math.Abs(_signedWidth), Math.Abs(_signedHeight));
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_selectionRectangle == null) return;

        if (_isSelecting && _inputSurface is not null && _inputSurface.IsMouseCaptured)
        {
            var currentMousePosition = e.GetPosition(_mainCanvas);

            //only left to right expands selection box
            _signedWidth = currentMousePosition.X - _startPoint.X;
            _signedHeight = currentMousePosition.Y - _startPoint.Y;

            _selectionRectangle.Width = Math.Abs(_signedWidth);
            _selectionRectangle.Height = Math.Abs(_signedHeight);

            var actualWidth = _signedWidth < 0 ? currentMousePosition.X : _startPoint.X;
            var actualHeight = _signedHeight < 0 ? currentMousePosition.Y : _startPoint.Y;
            Canvas.SetLeft(_selectionRectangle, actualWidth);
            Canvas.SetTop(_selectionRectangle, actualHeight);
        }
    }

    private void SetSelectionBoxStart()
    {
        if (_selectionRectangle is null) return;

        Canvas.SetLeft(_selectionRectangle, _startPoint.X);
        Canvas.SetTop(_selectionRectangle, _startPoint.Y);
    }

    private void ResetSelectionBox()
    {
        if (_selectionRectangle == null) return;
        Canvas.SetLeft(_selectionRectangle, 0);
        Canvas.SetTop(_selectionRectangle, 0);
        _selectionRectangle.Width = 0;
        _selectionRectangle.Height = 0;
    }

    private static bool IsImageHit(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is FrameworkElement { DataContext: ImageViewModel })
                return true;

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }
}
