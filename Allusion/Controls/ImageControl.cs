using System.Windows;
using System.Windows.Controls;

namespace Allusion.Controls;

public class ImageControl : UserControl
{
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
        nameof(IsSelected),
        typeof(bool),
        typeof(ImageControl),
        new PropertyMetadata(default(bool)));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
}