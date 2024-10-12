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
    private Rectangle _selectionRectangle;
    private Point _startPoint;
    private Canvas? _mainCanvas;

    private IEventAggregator? _events;

    protected override void OnAttached()
    {
        base.OnAttached();

        _events = IoC.Get<IEventAggregator>();

        if (AssociatedObject is Canvas canvas)
        {
            _mainCanvas = canvas;

            _mainCanvas.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            _mainCanvas.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            _mainCanvas.PreviewMouseMove += OnMouseMove;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (_mainCanvas == null) return;

        _mainCanvas.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
        _mainCanvas.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
        _mainCanvas.PreviewMouseMove -= OnMouseMove;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject.IsMouseCaptured)
        {
            CreateSelectionHitTest();

            if(_images.Any())
                _events.PublishOnBackgroundThreadAsync(new SelectionEvent(_images.ToArray(), SelectionType.Multi));

            AssociatedObject.ReleaseMouseCapture();
            ResetSelectionBox();
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(_mainCanvas);
        _images.Clear();
        //Check if mouse hits canvas only
        VisualTreeHelper.HitTest(_mainCanvas, null, CheckType,
            new GeometryHitTestParameters(new EllipseGeometry(e.GetPosition(_mainCanvas), 1, 1)));

        e.Handled = false;
    }
    private void CreateSelectionHitTest()
    {
        var hitRectangle = new RectangleGeometry()
        {
            Rect = new Rect(_startPoint, new Size(_selectionRectangle.ActualWidth, _selectionRectangle.ActualHeight))
        };

        var hitParameters = new GeometryHitTestParameters(hitRectangle);

        VisualTreeHelper.HitTest(_mainCanvas, null, CheckSelection, hitParameters);
    }

    private HitTestFilterBehavior FilterSelection(DependencyObject target)
    {
        if (target is Canvas { Name: "ImageCanvas" })
            return HitTestFilterBehavior.Continue;

        return HitTestFilterBehavior.ContinueSkipSelf;
    }

    private HitTestResultBehavior CheckSelection(HitTestResult result)
    {
        if (result.VisualHit is Border { DataContext: ImageViewModel tvm } && !_images.Contains(tvm))
            _images.Add(tvm);

        return HitTestResultBehavior.Continue;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_selectionRectangle == null) return;

        if (AssociatedObject.IsMouseCaptured)
        {
            var currentMousePosition = e.GetPosition(_mainCanvas);

            //only left to right expands selection box
            var width = currentMousePosition.X - _startPoint.X;
            var height = currentMousePosition.Y - _startPoint.Y;

            if (width > 0 && height > 0)
            {
                _selectionRectangle.Width = width;
                _selectionRectangle.Height = height;
            }
        }
    }



 

    private void SetSelectionBoxStart()
    {
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

    private HitTestResultBehavior CheckType(HitTestResult hit)
    {
        if (hit.VisualHit is Canvas canvas)
        {
            _events.PublishOnBackgroundThreadAsync(new ClearSelectionEvent(), new CancellationToken(true));
            canvas.Focus();
            AssociatedObject.CaptureMouse();
            _selectionRectangle = _mainCanvas.FindName("SelectionRectangle") as Rectangle;
            SetSelectionBoxStart();
        }

        return HitTestResultBehavior.Stop;
    }
}