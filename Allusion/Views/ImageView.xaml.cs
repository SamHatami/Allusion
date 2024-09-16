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
        public ImageView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as ImageViewModel;
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
