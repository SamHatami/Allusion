using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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

    private void MainView_OnDragEnter(object sender, DragEventArgs e)
    {
        this.Topmost = true;
    }

    private void MainView_OnDragLeave(object sender, DragEventArgs e)
    {
        this.Topmost = false;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        this.Topmost = this.Topmost != true;


        DisplayIcon.Foreground = this.Topmost ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Black);
    }

    private void ThemeSwitch_Click(object sender, RoutedEventArgs e)
    {
        // Identify the current theme based on the existing resource dictionaries
        var currentTheme = Application.Current.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.EndsWith("Dark.xaml"));

        // Clear existing dictionaries
        Application.Current.Resources.MergedDictionaries.Clear();

        // Add the opposite theme
        var newTheme = currentTheme != null ? "Light.xaml" : "Dark.xaml";
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"Themes/{newTheme}", UriKind.Relative) });

        // Re-add the global resources
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/Globals.xaml", UriKind.Relative) });
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/Controls.xaml", UriKind.Relative) });
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/SolidColorBrushes.xaml", UriKind.Relative) });

       
        this.InvalidateVisual(); // Forces the window to redraw
        this.UpdateLayout();     // Updates the layout to apply new styles
    }
}