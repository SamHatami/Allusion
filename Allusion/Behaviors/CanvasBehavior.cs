using Allusion.Core.Helpers;
using Allusion.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace Allusion.Behaviors;

public class CanvasBehavior : Behavior<UIElement>
{
    private MainViewModel? _mainViewModel;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is FrameworkElement element)
        {
            element.DataContextChanged += OnDataContextChanged;
            GetDataContext();
        }

        //AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.Drop += OnDrop;
        //AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        GetDataContext();
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (_mainViewModel == null) return;

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        var files = e.Data.GetData(DataFormats.FileDrop, true) as string[];

        var bitmaps = BitmapHelper.GetImagesFromUri(files);

        _mainViewModel.AddDropppedImages(bitmaps);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
        }
    }

    private void GetDataContext()
    {
        if (AssociatedObject is FrameworkElement { DataContext: MainViewModel mainViewModel })
            _mainViewModel = mainViewModel;
    }
}