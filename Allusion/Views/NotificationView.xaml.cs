using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Allusion.Views;

/// <summary>
/// Interaction logic for NotificationView.xaml
/// </summary>
public partial class NotificationView : UserControl
{
    public NotificationView()
    {
        InitializeComponent();

        Loaded += (s, e) => StartAutoDismissTimer();
    }

    private void StartAutoDismissTimer()
    {
        // Wait for 3 seconds, then start the fade-out animation
        var dismissTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        dismissTimer.Tick += (s, e) =>
        {
            dismissTimer.Stop();
            StartFadeOutAnimation();
        };
        dismissTimer.Start();
    }

    private void StartFadeOutAnimation()
    {
        // Create the fade-out animation
        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1.0, // Start from fully visible
            To = 0.0, // Fade to completely transparent
            Duration = TimeSpan.FromSeconds(2) // 1-second fade duration
        };

        // Start the animation on the window's Opacity property
        BeginAnimation(OpacityProperty, fadeOutAnimation);
    }
}