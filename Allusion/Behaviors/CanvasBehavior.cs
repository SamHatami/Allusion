using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Allusion.Controls;
using Allusion.ViewModels;
using Allusion.WPFCore.Extensions;
using Allusion.WPFCore.Helpers;
using Microsoft.Xaml.Behaviors;

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

        //AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.Drop += OnDrop;
        //AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        GetDataContext();
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (_mainViewModel == null) return;

        List<BitmapSource> bitmaps = new List<BitmapSource>();

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.None;
        }

        var bitmap = e.Data.GetBitmap();

        var files = e.Data.GetData(DataFormats.FileDrop, true) as string[];

        bitmaps = BitmapHelper.GetImagesFromUri(files).ToList();

        _mainViewModel.AddDropppedImages(bitmaps.ToArray());
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