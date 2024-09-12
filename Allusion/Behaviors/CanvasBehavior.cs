using Allusion.ViewModels;
using Allusion.WPFCore.Service;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using Allusion.WPFCore.Handlers;

namespace Allusion.Behaviors;

public class CanvasBehavior : Behavior<UIElement>
{
    private MainViewModel? _mainViewModel; //Replace this with an event aggregation

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is FrameworkElement element)
        {
            element.DataContextChanged += OnDataContextChanged;
            GetDataContext();
        }

        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.Drop += OnDrop;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.Drop -= OnDrop;
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
        }

        ArtBoardHandler.DroppedNewObjects(e.Data);
       
        //_mainViewModel.AddDropppedImages(bitmaps.ToArray());

        e.Handled = true;
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