using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Allusion.Views.Dialogs;

/// <summary>
/// Interaction logic for OperRefBoardView.xaml
/// </summary>
public partial class OpenRefBoardView : Window
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
}