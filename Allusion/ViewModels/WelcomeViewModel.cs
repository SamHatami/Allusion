using Caliburn.Micro;

namespace Allusion.ViewModels;

public class WelcomeViewModel : Screen
{
    public void Ok()
    {
        TryCloseAsync();
    }
}