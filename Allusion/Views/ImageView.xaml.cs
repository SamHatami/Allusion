using System.Windows;

namespace Allusion.Views
{
    public partial class ImageView : WPFCore.Controls.ImageControl
    {
        public ImageView()
        {
            InitializeComponent();
        }

        private void ImageView_OnDrop(object sender, DragEventArgs e)
        {
            e.Handled = false;
        }
    }
}
