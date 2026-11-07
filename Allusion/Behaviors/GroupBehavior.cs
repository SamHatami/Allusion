using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Allusion.ViewModels;
using Caliburn.Micro;
using Microsoft.Xaml.Behaviors;
using Allusion.WPFCore.Utilities;

namespace Allusion.Behaviors
{
    public class GroupBehavior : Behavior<Border>
    {
        private Canvas _mainCanvas;
        private IEventAggregator _events;
        private GroupViewModel? _groupViewModel;
        private PageViewModel? _page;
        private Point _startPoint;
        private Point[] _relativeImagePositions;
        private ImageViewModel[] _childImages;
        private bool _isDragging;
        private double _startScale = 1.0;
        private Point _groupCenter;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
            AssociatedObject.MouseRightButtonDown += OnMouseRightButtonDown;
            AssociatedObject.MouseWheel += OnMouseWheel;

            _events = IoC.Get<IEventAggregator>();
            _mainCanvas = GetMainCanvas(AssociatedObject);

            if (AssociatedObject.DataContext is GroupViewModel group)
            {
                _groupViewModel = group;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            AssociatedObject.MouseRightButtonDown -= OnMouseRightButtonDown;
            AssociatedObject.MouseWheel -= OnMouseWheel;
            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_groupViewModel == null) return;

            _page = GetPageViewModel();
            if (_page == null) return;

            // Selection logic
            if (!_page.SelectedGroups.Contains(_groupViewModel))
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    _page.AddToGroupSelection(_groupViewModel);
                }
                else
                {
                    _page.SetSingleGroupSelection(_groupViewModel);
                }
            }

            _childImages = [.. _groupViewModel.Images];
            _relativeImagePositions = new Point[_childImages.Length];
            _groupCenter = new Point(_groupViewModel.X + _groupViewModel.Width / 2, _groupViewModel.Y + _groupViewModel.Height / 2);

            for (int i = 0; i < _childImages.Length; i++)
            {
                _relativeImagePositions[i] = new Point(
                    _childImages[i].PosX - _groupViewModel.X,
                    _childImages[i].PosY - _groupViewModel.Y);
            }

            _startPoint = e.GetPosition(_mainCanvas);
            _isDragging = true;
            AssociatedObject.CaptureMouse();
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !AssociatedObject.IsMouseCaptured) return;

            var currentPoint = e.GetPosition(_mainCanvas);
            var delta = new Point(currentPoint.X - _startPoint.X, currentPoint.Y - _startPoint.Y);

            _groupViewModel.X += delta.X;
            _groupViewModel.Y += delta.Y;

            // Move child images with group
            for (int i = 0; i < _childImages.Length; i++)
            {
                _childImages[i].PosX = _groupViewModel.X + _relativeImagePositions[i].X;
                _childImages[i].PosY = _groupViewModel.Y + _relativeImagePositions[i].Y;
            }

            _startPoint = currentPoint;
            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;
            _isDragging = false;
            AssociatedObject.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Context menu for group (color, delete, etc.)
            e.Handled = true;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_groupViewModel == null || Keyboard.Modifiers != ModifierKeys.Control) return;

            var scaleFactor = e.Delta > 0 ? 1.1 : 0.9;
            var newWidth = _groupViewModel.Width * scaleFactor;
            var newHeight = _groupViewModel.Height * scaleFactor;

            if (newWidth < 50 || newHeight < 50) return;

            _groupViewModel.Width = newWidth;
            _groupViewModel.Height = newHeight;

            // Scale child images relative to group center
            for (int i = 0; i < _childImages.Length; i++)
            {
                var relX = _childImages[i].PosX - _groupCenter.X;
                var relY = _childImages[i].PosY - _groupCenter.Y;
                _childImages[i].PosX = _groupCenter.X + relX * scaleFactor;
                _childImages[i].PosY = _groupCenter.Y + relY * scaleFactor;
            }

            e.Handled = true;
        }

        private PageViewModel? GetPageViewModel()
        {
            DependencyObject dp = AssociatedObject;
            while (dp != null)
            {
                if (dp is FrameworkElement fe && fe.DataContext is PageViewModel page) return page;
                dp = VisualTreeHelper.GetParent(dp);
            }
            return null;
        }

        private Canvas GetMainCanvas(DependencyObject element)
        {
            while (element != null)
            {
                if (element is Canvas { Name: "ImageCanvas" } canvas) return canvas;
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }
    }
}