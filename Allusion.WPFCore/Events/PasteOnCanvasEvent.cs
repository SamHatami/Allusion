using System.Windows;

namespace Allusion.WPFCore.Events;

public class PasteOnCanvasEvent
{
    public Size CurrentWindowSize { get; }

    public PasteOnCanvasEvent(Size currentWindowSize)
    {
        CurrentWindowSize = currentWindowSize;
    }
}