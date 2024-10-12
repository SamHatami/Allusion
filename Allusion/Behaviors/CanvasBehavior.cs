using Allusion.ViewModels;
using Allusion.WPFCore.Service;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Allusion.Controls;
using Allusion.Events;
using Allusion.WPFCore.Events;
using Caliburn.Micro;

namespace Allusion.Behaviors;

public class CanvasBehavior : Behavior<UIElement>
{
    private IEventAggregator _events;
    protected override void OnAttached()
    {
        base.OnAttached();

        _events = IoC.Get<IEventAggregator>();
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        
        AssociatedObject.Drop += OnDrop;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not ImageControl)
            _events.PublishOnUIThreadAsync(new SelectionEvent(null, SelectionType.DeSelect));

        if (e.OriginalSource is not Canvas canvas) return;
        
        Keyboard.ClearFocus();
        canvas.Focus();

    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.Drop -= OnDrop;
    }
    
    private async void OnDrop(object sender, DragEventArgs e)
    {

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.None;
        }

        var dropPoint = e.GetPosition(AssociatedObject);
        var currentWindowSize = Window.GetWindow(AssociatedObject).RenderSize;
        await _events.PublishOnUIThreadAsync(new DragDropEvent(e.Data, dropPoint, currentWindowSize));

        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
        }
    }

    //private void GetDataContext()
    //{
    //    if (AssociatedObject is FrameworkElement { DataContext: PageViewModel mainViewModel })
    //        _pageViewModel = mainViewModel;
    //}
}