using Allusion.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Allusion.Views
{
    /// <summary>
    /// Interaction logic for PageView.xaml
    /// </summary>
    public partial class PageView : UserControl
    {
        private string oldDescription;

        public PageView()
        {
            InitializeComponent();
        }

        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            //Tried to do this with adorner, but having them active all the time renders all the thumbs on the top most layer
            //this is rather effective, except that you have to calculate the aspectRatio each time.
            //Not sure how to do it otherwise. But not the best place for it to live

            var thumb = sender as FrameworkElement;

            var contentControl = thumb.Parent as FrameworkElement;
            var vm = contentControl.DataContext as ImageViewModel;

            var aspectRatio = vm.AspectRatio;

            // Calculate new width and height

            var horizontalScaleFactor = 1 + e.HorizontalChange * 0.1 / contentControl.Width;
            var verticalScaleFactor = 1 + e.VerticalChange * 0.1 / contentControl.Height;

            var scaleFactor = Math.Min(horizontalScaleFactor, verticalScaleFactor);

            contentControl.Width = Math.Max(100, (contentControl.Width) * scaleFactor);
            contentControl.Height = Math.Max(100, (contentControl.Width) / aspectRatio);

            vm.Scale = contentControl.Width / vm.ImageSource.Width;
        }

        private void ResizeThumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Mouse.SetCursor(default);
        }

        private void ResizeThumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            Mouse.SetCursor(Cursors.SizeAll);
        }

        private void OnRenameLostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Visibility = Visibility.Collapsed;
        }

        private void OnRenameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).Visibility = Visibility.Collapsed;
            }

            if (e.Key == Key.Escape) //Can't figure out how to do this using the RenameBehavior
            {
                ((TextBox)sender).Text = oldDescription;
                ((TextBox)sender).Visibility = Visibility.Collapsed;

            }
        }

        private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox { Name: "RenameTextBox" } textbox)
            {
                oldDescription = textbox.Text;
            }
        }

    }
}
