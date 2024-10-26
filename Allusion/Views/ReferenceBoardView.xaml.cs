using Allusion.WPFCore.Service;
using Allusion.WPFCore.ValidationRules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private string oldRenameText = String.Empty;  
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

            if (e.Key == Key.Escape) //Can't figure out how to do this using the RenameBehavior
            {
                ((TextBox)sender).Text = oldRenameText;
                ((TextBox)sender).Visibility = Visibility.Collapsed;

            }
        }

        private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox { Name: "RenameTextBox" } textbox)
            {
                oldRenameText = textbox.Text;
            }
        }

        private void RenameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            
                ValidationResult result = FolderNameValidation.Validate(e.Text, CultureInfo.InvariantCulture);
                if (result != ValidationResult.ValidResult)
                {
                    // Log the error
                    StaticLogger.Error(result.ErrorContent.ToString());

                    // Mark the event as handled to prevent the illegal character from being added
                    e.Handled = true;
                }

        }
    }
}
