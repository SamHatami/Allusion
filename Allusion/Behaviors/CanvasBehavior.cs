using Allusion.ViewModels;
using Allusion.WPFCore.Service;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using Allusion.Controls;
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
        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.Drop += OnDrop;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not ImageControl)
        {
            _mainViewModel.DeselectAll();
        }
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

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (_mainViewModel == null) return;

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.None;
        }

        await _mainViewModel.BoardManager.GetDroppedImageItems(e.Data);

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