using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Allusion.Views
{
    /// <summary>
    /// Interaction logic for ReferenceBoardView.xaml
    /// </summary>
    public partial class ReferenceBoardView : UserControl
    {
        public ReferenceBoardView()
        {
            InitializeComponent();
        }

        private void OnRenameLostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Visibility = Visibility.Collapsed;
        }

        private void OnRenameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).Visibility = Visibility.Collapsed;
            }
        }

    }
}
