using System.Collections.Specialized;
using System.Net.Mime;
using Allusion.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FontAwesome.Sharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using Thumb = System.Windows.Controls.Primitives.Thumb;
using System.Windows.Data;

namespace Allusion.Views;

/// <summary>
/// Interaction logic for PageView.xaml
/// </summary>
public partial class PageView : UserControl
{
    private string _oldDescription;
    private FrameworkElement _dragIcon;
    private PageViewModel _pageViewModel;
    public PageView()
    {
        InitializeComponent();
        Loaded += OnLoaded;

    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _pageViewModel = this.DataContext as PageViewModel;
        _pageViewModel.Board.Pages.CollectionChanged += OnPageCollectionChanged();
    }

    private NotifyCollectionChangedEventHandler? OnPageCollectionChanged()
    {
        var contextMenu = (ContextMenu)ImageCanvas.FindName("ImageContextMenu");

        if (contextMenu == null) return null;

        contextMenu.Items.Refresh();

        return null;
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

        contentControl.Width = Math.Max(100, contentControl.Width * scaleFactor);
        contentControl.Height = Math.Max(100, contentControl.Width / aspectRatio);

        vm.Scale = contentControl.Width / vm.ImageSource.Width;


    }

    private void ResizeThumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
    {
        Mouse.SetCursor(default);

        if (_dragIcon != null)
        {
            _dragIcon.Visibility = Visibility.Hidden;
        }
    }

    private void ResizeThumb_OnDragStarted(object sender, DragStartedEventArgs e)
    {
        Mouse.SetCursor(Cursors.None);

        var thumb = sender as Thumb;
        thumb.Cursor = Cursors.None;
        if (thumb == null) return;
        var parentGrid = thumb.Parent as FrameworkElement;

        // Locate the DragIcon inside the DataTemplate by finding it in the parentGrid
        _dragIcon = parentGrid?.FindName("DragIcon") as FrameworkElement;

        if (_dragIcon != null)
        {
            _dragIcon.Visibility = Visibility.Visible;
        }
    }

    private void OnRenameLostFocus(object sender, RoutedEventArgs e)
    {
        ((TextBox)sender).Visibility = Visibility.Collapsed;
    }

    private void OnRenameKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) ((TextBox)sender).Visibility = Visibility.Collapsed;

        if (e.Key == Key.Escape) //Can't figure out how to do this using the RenameBehavior
        {
            ((TextBox)sender).Text = _oldDescription;
            ((TextBox)sender).Visibility = Visibility.Collapsed;
        }
    }

    private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox { Name: "RenameTextBox" } textbox) _oldDescription = textbox.Text;
    }
}