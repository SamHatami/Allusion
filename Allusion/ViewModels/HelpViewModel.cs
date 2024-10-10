using Caliburn.Micro;

namespace Allusion.ViewModels;

public class HelpViewModel : Screen
{
    public void Close()
    {
        TryCloseAsync();
    }
}