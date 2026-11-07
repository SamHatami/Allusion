using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Allusion.Events;
using Allusion.ViewModels;
using Caliburn.Micro;
using Microsoft.Xaml.Behaviors;
using Allusion.WPFCore.Interfaces;

namespace Allusion.Behaviors
{
    public class ReorderTabBehavior : Behavior<Button>
    {
        private Point _startPoint;
        private bool _isDragging;
        private IEventAggregator _events;
        private PageViewModel _page;
        private Panel _parentPanel;

        protected override void OnAttached()
        {
            base.OnAttached();
            _events = IoC.Get<IEventAggregator>();
            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove += OnMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            if (AssociatedObject.DataContext is PageViewModel page)
                _page = page;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove -= OnMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(AssociatedObject);
            _isDragging = true;
            AssociatedObject.CaptureMouse();
            AssociatedObject.Opacity = 0.7;
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !AssociatedObject.IsMouseCaptured) return;
            // Drag visual handled by opacity; reorder on drop via event
            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;
            _isDragging = false;
            AssociatedObject.ReleaseMouseCapture();
            AssociatedObject.Opacity = 1.0;

            // Compute nearest sibling index in parent panel for reorder
            if (AssociatedObject.Parent is Panel panel && _page != null)
            {
                int current = panel.Children.IndexOf(AssociatedObject);
                var mousePos = e.GetPosition(panel);
                int target = current;
                for (int i = 0; i < panel.Children.Count; i++)
                {
                    var child = panel.Children[i];
                    var childRect = VisualTreeHelper.GetDescendantBounds(child);
                    // Simplified: drop after nearest center; use approximate
                    if (mousePos.X > childRect.TopLeft.X + childRect.Width / 2)
                        target = i + 1;
                }
                if (target != current && target >= 0)
                    _events.PublishOnUIThreadAsync(new ReorderTabEvent(_page, target));
            }
            e.Handled = true;
        }
    }
}
