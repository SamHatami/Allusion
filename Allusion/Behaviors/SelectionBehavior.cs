using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Allusion.Events;
using Allusion.ViewModels;
using Caliburn.Micro;
using Microsoft.Xaml.Behaviors;

namespace Allusion.Behaviors;

public class SelectionBehavior : Behavior<UIElement>
{
    private ImageViewModel? _imageViewModel; //Byt ut till ISelectable istället
    private IEventAggregator? _events;

    protected override void OnAttached()
    {
        base.OnAttached();
        _events = IoC.Get<IEventAggregator>();;
        AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;

        if (AssociatedObject is Grid gridContainer && gridContainer.Children[1] is Border border)
            _imageViewModel = border.DataContext as ImageViewModel;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {

        _events.PublishOnBackgroundThreadAsync(new SelectionEvent([_imageViewModel]));
        AssociatedObject.ReleaseMouseCapture();

        e.Handled = false;
    }

}