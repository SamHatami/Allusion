using System.Diagnostics;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Allusion.Views;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Allusion.Behaviors;

public class FocusBehavior : Behavior<Image>
{
    private Canvas? _mainCanvas;
    private TransformGroup _transform;
    private ScaleTransform _scaling;
    private TranslateTransform _translating;
    private Point _currentMousePos;
    private Point _startPosition;
    private Rectangle? _zoomBox;
    private double _aspectRatio;
    private FocusView? _focusView;
    protected override void OnAttached()
    {
        base.OnAttached();

        _transform = new TransformGroup();
        _scaling = new ScaleTransform();
        _scaling.ScaleX = _scaling.ScaleY = 1;
        _translating = new TranslateTransform();
        _transform.Children.Add(_scaling);
        _transform.Children.Add(_translating);

        AssociatedObject.RenderTransform = _transform;
        
        AttachToEvents();
    }

    private void AttachToEvents()
    {
        AssociatedObject.Loaded += OnImageLoaded;
        AssociatedObject.MouseWheel += OnScrollWheel;
        AssociatedObject.MouseDown += OnMouseDown;
        AssociatedObject.MouseUp += OnMouseUp;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        AssociatedObject.MouseRightButtonDown += OnRightMouseDown;
        AssociatedObject.MouseRightButtonUp += OnRightMouseUp;

    }

    private void OnImageLoaded(object sender, RoutedEventArgs e)
    {
        _mainCanvas = GetMainCanvas(AssociatedObject, "FocusCanvas");
        if (_mainCanvas is null) return;

        _zoomBox = _mainCanvas.FindName("ZoomBox") as Rectangle;
        _focusView = Window.GetWindow(AssociatedObject) as FocusView ?? GetFocusView();
        if (_focusView is null) return;

        ResetToFitWindow();
        _focusView.SizeChanged += OnWindowSizeChanged;

    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_scaling.ScaleY == 1)
            ResetToFitWindow();
    }

    private void ResetToFitWindow()
    {
        if (_mainCanvas is null || _focusView is null) return;

        var windowWidth = _focusView.ActualWidth;
        var windowHeight = Math.Max(0, _focusView.ActualHeight - SystemParameters.CaptionHeight - 1);
        if (windowWidth <= 0 || windowHeight <= 0) return;

        _mainCanvas.Width = windowWidth;
        _mainCanvas.Height = windowHeight;
        AssociatedObject.Width = windowWidth;
        AssociatedObject.Height = windowHeight;

        _scaling.ScaleX = 1;
        _scaling.ScaleY = 1;
        _translating.X = 0;
        _translating.Y = 0;

        Canvas.SetLeft(AssociatedObject, 0);
        Canvas.SetTop(AssociatedObject, 0);
        ResetSelectionBox();
    }

    private FocusView? GetFocusView()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window is FocusView focusView)
                return focusView;
        }
        return null;
    }

    private void OnRightMouseUp(object sender, MouseButtonEventArgs e)
    {
        double left = Canvas.GetLeft(_zoomBox);
        double top = Canvas.GetTop(_zoomBox);
        double width = _zoomBox.Width;
        double height = _zoomBox.Height;

        e.Handled = true;
        ResetSelectionBox();
    }

    private void OnRightMouseDown(object sender, MouseButtonEventArgs e)
    {
        _startPosition = e.GetPosition(_mainCanvas);
        _aspectRatio = AssociatedObject.ActualWidth / AssociatedObject.ActualHeight;
        SetSelectionBoxStart();
        AssociatedObject.CaptureMouse();


        e.Handled = true;
    }


    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= OnImageLoaded;
        AssociatedObject.MouseWheel -= OnScrollWheel;
        AssociatedObject.MouseDown -= OnMouseDown;
        AssociatedObject.MouseUp -= OnMouseUp;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
        AssociatedObject.MouseRightButtonDown -= OnRightMouseDown;
        AssociatedObject.MouseRightButtonUp -= OnRightMouseUp;
        if (_focusView is not null)
            _focusView.SizeChanged -= OnWindowSizeChanged;

        base.OnDetaching();

    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        _currentMousePos = e.GetPosition(_mainCanvas);
    }


    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!AssociatedObject.IsMouseCaptured) return;

        if(e.MiddleButton == MouseButtonState.Pressed)
        {
            PanImage(e.GetPosition(_mainCanvas));
        }

        if (e.RightButton == MouseButtonState.Pressed)
        {
            CreateZoomBox(e.GetPosition(_mainCanvas));
        }

    }

    private void CreateZoomBox(Point currentMousePosition)
    {
        var signedWidth = currentMousePosition.X - _startPosition.X;
        var signedHeight = currentMousePosition.Y - _startPosition.Y;

        double aspectWidth = Math.Abs(signedWidth);
        double aspectHeight = Math.Abs(signedHeight); // Maintain aspect ratio

        _zoomBox.Width = aspectWidth;
        _zoomBox.Height = aspectHeight;

        var actualLeft = signedWidth < 0 ? currentMousePosition.X : _startPosition.X;
        var actualTop = signedHeight < 0 ? currentMousePosition.Y - aspectHeight + _zoomBox.Height : _startPosition.Y;

        Canvas.SetLeft(_zoomBox, actualLeft);
        Canvas.SetTop(_zoomBox, actualTop);

        Trace.WriteLine(_zoomBox.ActualWidth);
        Trace.WriteLine(_zoomBox.ActualHeight);
    }

    private void PanImage(Point mousePosition)
    {

        double newTranslateX = mousePosition.X - _startPosition.X;
        double newTranslateY = mousePosition.Y - _startPosition.Y;

        // Get the scaled width and height of the image
        double scaledWidth = AssociatedObject.ActualWidth * _scaling.ScaleX;
        double scaledHeight = AssociatedObject.ActualHeight * _scaling.ScaleY;

        // Get the bounds of the canvas
        double canvasWidth = _mainCanvas.ActualWidth;
        double canvasHeight = _mainCanvas.ActualHeight;

        // Calculate the min and max bounds for translation
        double minX = Math.Min(0, canvasWidth - scaledWidth);
        double maxX = Math.Max(0, canvasWidth - scaledWidth);
        double minY = Math.Min(0, canvasHeight - scaledHeight);
        double maxY = Math.Max(0, canvasHeight - scaledHeight);

        // Clamp the translation values within the bounds
        _translating.X = Math.Clamp(newTranslateX, minX, maxX);
        _translating.Y = Math.Clamp(newTranslateY, minY, maxY);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {



        if (e is { ChangedButton: MouseButton.Middle, ClickCount: 2 })
        {
            ResetToFitWindow();
        }

        if (e is { ChangedButton: MouseButton.Middle, ClickCount: 1 })
        {
    
            var mousePosition = e.GetPosition(_mainCanvas);
            _startPosition = new Point(mousePosition.X - _translating.X, mousePosition.Y - _translating.Y);
            AssociatedObject.CaptureMouse();
        }


        e.Handled = true;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            AssociatedObject.ReleaseMouseCapture();
        }

    }

    private void OnScrollWheel(object sender, MouseWheelEventArgs e)
    {
        var mousePos = e.GetPosition(_mainCanvas);

        double scaleChange = e.Delta / 1000.0; 
        double newScaleX = _scaling.ScaleX + scaleChange;
        double newScaleY = _scaling.ScaleY + scaleChange;

        if (newScaleX >= 1 && newScaleY >= 1)
        {
            // Calculate the new translation based on the scaling change
            double scaleFactorX = newScaleX / _scaling.ScaleX;
            double scaleFactorY = newScaleY / _scaling.ScaleY;

            // Adjust translation to keep the zoom centered under the mouse
            _translating.X = (_translating.X - mousePos.X) * scaleFactorX + mousePos.X;
            _translating.Y = (_translating.Y - mousePos.Y) * scaleFactorY + mousePos.Y;

            // Apply the new scaling
            _scaling.ScaleX = newScaleX;
            _scaling.ScaleY = newScaleY;
        }

        else
        {
            Canvas.SetLeft(AssociatedObject, 0);
            Canvas.SetTop(AssociatedObject, 0);
        }


        e.Handled = true;

    }


    private void SetSelectionBoxStart()
    {
        Canvas.SetLeft(_zoomBox, _startPosition.X);
        Canvas.SetTop(_zoomBox, _startPosition.Y);
    }

    private void ResetSelectionBox()
    {
        if (_zoomBox == null) return;
        Canvas.SetLeft(_zoomBox, 0);
        Canvas.SetTop(_zoomBox, 0);
        _zoomBox.Width = 0;
        _zoomBox.Height = 0;
    }
    private Canvas GetMainCanvas(DependencyObject? element, string canvasName)
    {
        while (element != null)
        {
            if (element is Canvas canvas && canvas.Name == canvasName) return canvas;
            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }

    //Test
    //https://stackoverflow.com/questions/4474670/how-to-catch-the-ending-resize-window

    const int WM_SIZING = 0x214;
    const int WM_EXITSIZEMOVE = 0x232;
    private static bool WindowWasResized = false;


    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_SIZING)
        {

            if (WindowWasResized == false)
            {

                //    'indicate the the user is resizing and not moving the window
                WindowWasResized = true;
            }
        }

        if (msg == WM_EXITSIZEMOVE)
        {

            // 'check that this is the end of resize and not move operation          
            if (WindowWasResized == true)
            {

                // your stuff to do 
                Console.WriteLine("End");

                // 'set it back to false for the next resize/move
                WindowWasResized = false;
            }
        }

        return IntPtr.Zero;
    }

}

