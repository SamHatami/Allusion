using System.Windows;
using System.Windows.Controls;
using Allusion.ViewModels;
using Allusion.Views;
using Allusion.WPFCore.Board;
using Caliburn.Micro;
using Microsoft.Xaml.Behaviors;
using System.Windows.Input;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;

namespace Allusion.Behaviors
{
    public class TabButtonBehavior : Behavior<UIElement>
    {
        private IEventAggregator _events;
        private IPageViewModel _dataContext;

        protected override void OnAttached()
        {
            base.OnAttached();

            _events = IoC.Get<IEventAggregator>();
            AssociatedObject.DragEnter += OnDragEnter;
            AssociatedObject.Drop += OnDrop;

            if (AssociatedObject is Button { DataContext: PageViewModel page })
                _dataContext = page;


        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            // Check if the DataContext is PageViewModel and if the page is not selected
            if (AssociatedObject.AllowDrop)
            {
                // This indicates that the data is acceptable to be dropped here
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.DragEnter -= OnDragEnter;
            AssociatedObject.Drop -= OnDrop;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (AssociatedObject is Button { DataContext: PageViewModel { PageIsSelected: true } }) return;


            if (e.Data.GetDataPresent("ImageVM") && AssociatedObject.AllowDrop)
            {
                var image = e.Data.GetData("ImageVM") as ImageViewModel;
                
                image.Dropped = true;

                _events.PublishOnBackgroundThreadAsync(new DropOnTabEvent(image, _dataContext));
            }

            Mouse.OverrideCursor = null;

        }
    }
}
