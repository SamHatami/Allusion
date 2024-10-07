using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Allusion.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for NewRefBoardView.xaml
    /// </summary>
    public partial class NewRefBoardView : Window
    {
        public NewRefBoardView()
        {
            InitializeComponent();
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textbox) return;
            textbox.Focus();
            Keyboard.Focus(textbox);
        }
    }
}
