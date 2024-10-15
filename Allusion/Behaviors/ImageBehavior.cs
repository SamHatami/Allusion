using Allusion.ViewModels;
using Allusion.WPFCore.Utilities;
using Caliburn.Micro;
using FontAwesome.Sharp;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Allusion.Behaviors;

public class ImageBehavior : Behavior<UIElement>
{
    private Canvas _mainCanvas;
    private Point[] _relativePositions;
    private List<ContentPresenter> _contentPresenters;
    private IEventAggregator _events;
    private Image _dragIcon;
    private ImageViewModel? _imageViewModel;
    private ImageViewModel[] _selectedImages;
    private DragDropEffects dropResult;
    private bool _isInDropMode;
    private DragDropEffects dropEffect;
    private Point[] _originalPositions;
    private PageViewModel? _page;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (!(VisualTreeHelper.GetParent(AssociatedObject) is ContentPresenter contentPresenter)) return; //onödigt?

        _mainCanvas = GetMainCanvas(AssociatedObject);
        _contentPresenters = VisualTreeHelpers.FindContentPresentersForImageViews(_mainCanvas);

        SetDataContextAndEvents();

        if (AssociatedObject is Grid gridContainer && gridContainer.Children[1] is Border border)
            _imageViewModel = border.DataContext as ImageViewModel;

        _isInDropMode = _imageViewModel.Dropped;

        _events = IoC.Get<IEventAggregator>();
        _events.SubscribeOnUIThread(this); //Listen to FileDropEvent to if the imageviewmodel is the same as the one here, otherwise

        AttachEventHandlers();
    }

    private void AttachEventHandlers()
    {
        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
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
        if (!_page.SelectedImages.Contains(_imageViewModel))
            //TODO: Ersätt detta med en selectionService. EventAggregatorn hinner inte ifatt innan CaptureMouse kommer igång
            //Task.Run(async => await...) fungerar inte heller
            _page.SetSingleSelection(_imageViewModel);

        var startPosition = e.GetPosition(_mainCanvas);

        SetPositions(startPosition);
        CreateDragIcon(startPosition);
        Mouse.OverrideCursor = Cursors.None;

        AssociatedObject.Focus();
        AssociatedObject.CaptureMouse();

        e.Handled = true;
    }

    private void SetPositions(Point startPosition)
    {
        _selectedImages = _page.SelectedImages.ToArray();
        _relativePositions = new Point[_selectedImages.Length];
        _originalPositions = new Point[_selectedImages.Length];
        for (var i = 0; i < _selectedImages.Length; i++)
        {
            _relativePositions[i].X = _selectedImages[i].PosX - startPosition.X;
            _relativePositions[i].Y = _selectedImages[i].PosY - startPosition.Y;

            _originalPositions[i].X = _selectedImages[i].PosX;
            _originalPositions[i].Y = _selectedImages[i].PosY;
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (AssociatedObject.IsMouseCaptured)

        {
            for (var i = 0; i < _selectedImages.Length; i++)
                MoveImage(_selectedImages[i], _originalPositions[i], _relativePositions[i], e.GetPosition(_mainCanvas));

            MoveDragIcon(e.GetPosition(_mainCanvas));
        }

        e.Handled = true;
    }

    private void MoveImage(ImageViewModel image, Point originalPos, Point relativePos, Point mousePos)
    {
        var newLeft = mousePos.X + relativePos.X;
        image.PosX = newLeft < 0 ? 0 : newLeft;

        var newTop = mousePos.Y + relativePos.Y;
        image.PosY = newTop;

        if (newTop < -10 && Mouse.LeftButton == MouseButtonState.Pressed)
        {
            // If the image goes beyond the top of the canvas, enable drop mode
            EnterDropMode();

            if (!_isInDropMode && dropEffect == DragDropEffects.None) //Replace with bool
            {
                image.PosX = originalPos.X;
                image.PosY = originalPos.Y;
                Mouse.OverrideCursor = null;
                ExitDropMode();
            }
        }
        else
        {
            // If the image is within bounds, reset to normal behavior
            ExitDropMode();
        }
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
        Canvas.SetLeft(_dragIcon, position.X);
        Canvas.SetTop(_dragIcon, position.Y);
    }

    // Remove the drag icon when resizing is complete
    private void RemoveDragIcon()
    {
        _mainCanvas.Children.Remove(_dragIcon);
        _dragIcon = null;
    }

    private void EnterDropMode()
    {
        _contentPresenters.ForEach(i => i.Opacity = 0.5); //need to filter out those who are not selected

        var data = new DataObject();
        data.SetData("ImageVM", _selectedImages);
        dropEffect = DragDrop.DoDragDrop(AssociatedObject, data, DragDropEffects.Move);
    }

    private void ExitDropMode()
    {
        _contentPresenters.ForEach(i => i.Opacity = 1.0); // Reset opacity
    }

    private void SetDataContextAndEvents()
    {
        _page = _mainCanvas.DataContext as PageViewModel;
        _selectedImages = _page.Images.Where(i => i.IsSelected).ToArray();
    }
}