using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Allusion.Controls;
using Allusion.ViewModels;

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
            _window.SizeChanged += WindowSizeChanged;


        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_window is null or MainView) return;

            if (e.WidthChanged)
            {
                _window.Height = _window.Width / _aspectRatio;
            }
            else if (e.HeightChanged)
            {
                _window.Width = _window.Height * _aspectRatio;
            }
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
