using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Allusion.Controls
{
    public partial class GroupBox : UserControl
    {
        public GroupBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(GroupBox), new PropertyMetadata("Group"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
    }
}
