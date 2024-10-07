using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Allusion.Behaviors;

public class RenameBehavior : Behavior<UIElement>
{
    public static readonly DependencyProperty TextBoxProperty =
        DependencyProperty.Register(nameof(TextBox), typeof(TextBox), typeof(RenameBehavior));

    public static readonly DependencyProperty NrClickToEditProperty = DependencyProperty.Register(
        nameof(IsSingleClick), typeof(bool), typeof(RenameBehavior), new PropertyMetadata(default(bool)));

    public bool IsSingleClick
    {
        get => (bool)GetValue(NrClickToEditProperty);
        set => SetValue(NrClickToEditProperty, value);
    }

    public TextBox TextBox
    {
        get => (TextBox)GetValue(TextBoxProperty);
        set => SetValue(TextBoxProperty, value);
    }
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.MouseLeftButtonDown += OnMouseClick;
        AssociatedObject.PreviewMouseLeftButtonDown += OnePreviewClick;
        AssociatedObject.PreviewKeyDown += OnKeyDown;
        AssociatedObject.LostFocus += OnLostFocus;
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        TextBox.Visibility = Visibility.Collapsed;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            TextBox.Visibility = Visibility.Collapsed;
            Keyboard.ClearFocus();
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseLeftButtonDown -= OnMouseClick;
        AssociatedObject.PreviewMouseLeftButtonDown -= OnePreviewClick;
    }

    private void OnePreviewClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        //if mouseclick did not hit textbox or label, unfocus the textbox.
        //Hit detection using VisualTreeHelepr.HitTest
    }

    private void OnMouseClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        int clickCount = 2;
        if (IsSingleClick)
            clickCount = 1;

        if (e.ClickCount == clickCount && TextBox is not null)
        {
            TextBox.Visibility = Visibility.Visible;
            TextBox.Focus();
            e.Handled = true;
        }
    }
}