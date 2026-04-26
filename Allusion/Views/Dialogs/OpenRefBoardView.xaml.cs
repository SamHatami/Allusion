using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Allusion.Views.Dialogs;

/// <summary>
/// Interaction logic for OperRefBoardView.xaml
/// </summary>
public partial class OpenRefBoardView : UserControl
{
    public OpenRefBoardView()
    {
        InitializeComponent();
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ListBox { Items.Count: > 0 } boardList) return;

        boardList.SelectedIndex = 0;

        if (boardList.ItemContainerGenerator.ContainerFromIndex(0) is not ListBoxItem firstItem) return;
        // Set focus to the first item
        firstItem.Focus();
        Keyboard.Focus(firstItem);
    }

    private void OnRefBoardsPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox) return;

        var item = FindParent<ListBoxItem>(e.OriginalSource as DependencyObject);
        if (item is null) return;

        item.IsSelected = true;
        item.Focus();
    }

    private static T? FindParent<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T parent)
                return parent;

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
