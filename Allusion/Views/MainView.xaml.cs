using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Allusion.ViewModels;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Allusion.Views;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnWindowInitialized(object sender, EventArgs e)
    {
        RefreshMaximizeRestoreButton();
    }

    private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }

    private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        else if (Application.Current.MainWindow.WindowState == WindowState.Normal)
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void RefreshMaximizeRestoreButton()
    {
        if (WindowState == WindowState.Maximized)
        {
            MaximizeButton.Visibility = Visibility.Collapsed;
            RestoreButton.Visibility = Visibility.Visible;
        }
        else
        {
            MaximizeButton.Visibility = Visibility.Visible;
            RestoreButton.Visibility = Visibility.Collapsed;
        }
    }

    private void WindowStateChangedHandler(object sender, EventArgs e)
    {
        RefreshMaximizeRestoreButton();
    }
}