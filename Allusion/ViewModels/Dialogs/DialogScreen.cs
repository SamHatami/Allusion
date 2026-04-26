using Caliburn.Micro;

namespace Allusion.ViewModels.Dialogs;

public abstract class DialogScreen : Screen
{
    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            NotifyOfPropertyChange(nameof(Title));
        }
    }

    public DialogResultType DialogResult { get; protected set; } = DialogResultType.Cancel;

    protected Task CloseWithResult(DialogResultType result)
    {
        DialogResult = result;
        return TryCloseAsync(result is DialogResultType.Ok or DialogResultType.Yes);
    }
}
