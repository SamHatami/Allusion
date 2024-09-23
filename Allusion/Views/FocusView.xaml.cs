using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Allusion.Controls;
using Allusion.ViewModels;
using Microsoft.VisualBasic.Logging;

namespace Allusion.Views
{
    /// <summary>
    /// Interaction logic for ImageView.xaml
    /// </summary>
    public partial class FocusView : Window
    {
        private FocusViewModel _viewModel;
        private double _aspectRatio;
        private Window? _window;

        public FocusView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as FocusViewModel;
            _aspectRatio = _viewModel.AspectRatio;
            _window = Window.GetWindow(this);

            if (_window is not FocusView) return;
            _window.MouseDown += OnWindowMouseDown;
            _window.SourceInitialized += OnHostingWindowInit;
            
            _window.Width = _window.Height * _aspectRatio-(int)SystemParameters.WindowCaptionHeight;
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }


        private void OnHostingWindowInit(object? sender, EventArgs e)
        {
            WindowAspectRatio.Register((Window)sender);

        }


    }
}
