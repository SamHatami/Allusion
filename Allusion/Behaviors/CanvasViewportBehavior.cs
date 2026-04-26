using System.Windows;
using System.Windows.Input;
using Allusion.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace Allusion.Behaviors;

public class CanvasViewportBehavior : Behavior<FrameworkElement>
{
    private bool _isPanning;
    private Point _lastPanPoint;
    private Cursor? _previousCursor;
    private PageViewModel? _page;

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.DataContextChanged += OnDataContextChanged;
        AssociatedObject.PreviewMouseWheel += OnPreviewMouseWheel;
        AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
        AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        AssociatedObject.PreviewMouseUp += OnPreviewMouseUp;
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        AssociatedObject.LostMouseCapture += OnLostMouseCapture;

        SetPage(AssociatedObject.DataContext);
    }

    protected override void OnDetaching()
    {
        AssociatedObject.DataContextChanged -= OnDataContextChanged;
        AssociatedObject.PreviewMouseWheel -= OnPreviewMouseWheel;
        AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
        AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
        AssociatedObject.PreviewMouseUp -= OnPreviewMouseUp;
        AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        AssociatedObject.LostMouseCapture -= OnLostMouseCapture;

        EndPan();

        base.OnDetaching();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        SetPage(e.NewValue);
    }

    private void SetPage(object? dataContext)
    {
        _page = dataContext as PageViewModel;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_page == null) return;

        _page.Viewport.ZoomAt(e.GetPosition(AssociatedObject), e.Delta);
        e.Handled = true;
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_page == null) return;

        if (e.ChangedButton == MouseButton.Middle || (e.ChangedButton == MouseButton.Left && Keyboard.IsKeyDown(Key.Space)))
        {
            BeginPan(e.GetPosition(AssociatedObject));
            e.Handled = true;
        }
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning || _page == null) return;

        var currentPoint = e.GetPosition(AssociatedObject);
        _page.Viewport.PanBy(currentPoint - _lastPanPoint);
        _lastPanPoint = currentPoint;
        e.Handled = true;
    }

    private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isPanning) return;

        if (e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Left)
        {
            EndPan();
            e.Handled = true;
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_page == null) return;

        if (e.Key == Key.D0 && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            _page.Viewport.Reset();
            e.Handled = true;
        }
    }

    private void OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        EndPan();
    }

    private void BeginPan(Point point)
    {
        _isPanning = true;
        _lastPanPoint = point;
        _previousCursor = AssociatedObject.Cursor;
        AssociatedObject.Cursor = Cursors.SizeAll;
        AssociatedObject.Focus();
        AssociatedObject.CaptureMouse();
    }

    private void EndPan()
    {
        if (!_isPanning) return;

        _isPanning = false;
        AssociatedObject.Cursor = _previousCursor;

        if (AssociatedObject.IsMouseCaptured)
            AssociatedObject.ReleaseMouseCapture();
    }
}
