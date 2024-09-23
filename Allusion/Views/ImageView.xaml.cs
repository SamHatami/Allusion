using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Allusion.Controls;
using Allusion.ViewModels;
using Microsoft.VisualBasic.Logging;
using Window = System.Windows.Window;

namespace Allusion.Views
{
    /// <summary>
    /// Interaction logic for ImageView.xaml
    /// </summary>
    public partial class ImageView : ImageControl
    {
        private ImageViewModel _viewModel;
        private double _aspectRatio;
        private Window? _window;

        public ImageView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as ImageViewModel;
            _aspectRatio = _viewModel.ImageSource.Width/_viewModel.ImageSource.Height;
            _window = Window.GetWindow(this);

            if (_window is MainView) return;

            _window.Dispatcher.Invoke(() =>
            {
                _window.WindowState = WindowState.Normal;  // Ensure the window isn't maximized
                _window.Height = 300;  // Set a default height
                _window.Width = _window.Height * _aspectRatio;  // Adjust width to respect aspect ratio
            });
            _window.SourceInitialized += OnHostingWindowInit;

        }
        private void OnHostingWindowInit(object? sender, EventArgs e)
        {
            WindowAspectRatio.Register((Window)sender);



        }

        private void BoundImage_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var modifiers = Keyboard.Modifiers;
            ImageClick(modifiers);
        }

        private void ImageClick(ModifierKeys modifiers)
        {
            _viewModel.ImageClick(modifiers);
        }
    }
}
