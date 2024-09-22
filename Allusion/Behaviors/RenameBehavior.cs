using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Allusion.Behaviors;

public class RenameBehavior : Behavior<Label>
{
    public static readonly DependencyProperty TextBoxProperty =
        DependencyProperty.Register(nameof(TextBox), typeof(TextBox), typeof(RenameBehavior));

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
    

        if (e.ClickCount == 2 && TextBox is not null)
        {
            Trace.WriteLine("doubleclicked");
            TextBox.Visibility = Visibility.Visible;
            TextBox.Focus();
            e.Handled = true;
        }
    }
}