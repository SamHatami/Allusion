﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Allusion.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            //Tried to do this with adorner, but having them active all the time renders all the thumbs on the top most layer
            //this is rather effective, except that you have to calculate the aspectRatio each time.
            //Not sure how to do it otherwise.

            var thumb = sender as FrameworkElement;
            var contentControl = thumb.Parent as FrameworkElement;
            var aspectRatio = contentControl.Height / contentControl.Width;
            // Calculate new width and height

            var horizontalScaleFactor = 1 + e.HorizontalChange * 0.1 / contentControl.Width;
            var verticalScaleFactor = 1 + e.VerticalChange * 0.1 / contentControl.Height;

            var scaleFactor = Math.Min(horizontalScaleFactor, verticalScaleFactor);

            contentControl.Width = Math.Max(0, (contentControl.Width) * scaleFactor);
            contentControl.Height = Math.Max(0, (contentControl.Width) / aspectRatio);
        }
    }
}
