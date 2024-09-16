namespace Allusion.WPFCore.Events;

public class BoardIsModfiedEvent
{
    public bool IsModfied { get; set; }

    public BoardIsModfiedEvent(bool isModfied)
    {
        IsModfied = isModfied;
    }
}