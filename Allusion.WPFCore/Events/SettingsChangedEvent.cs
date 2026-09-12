namespace Allusion.WPFCore.Events;

public class SettingsChangedEvent
{
    public double ImageBorderThickness { get; }

    public SettingsChangedEvent(double imageBorderThickness)
    {
        ImageBorderThickness = imageBorderThickness;
    }
}
