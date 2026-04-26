using Caliburn.Micro;

namespace Allusion.ViewModels.Dialogs;

public class RemoveRefBoardViewModel : DialogScreen
{
    private bool _deleteLocalFiles;

    public string Message { get; }

    public bool DeleteLocalFiles
    {
        get => _deleteLocalFiles;
        set
        {
            _deleteLocalFiles = value;
            NotifyOfPropertyChange(nameof(DeleteLocalFiles));
        }
    }

    public RemoveRefBoardViewModel(string boardName)
    {
        Title = "Remove Board";
        Message = $"Remove '{boardName}' from the board list?";
    }

    public RemoveRefBoardViewModel() : this("selected board")
    {
    }

    public Task Remove()
    {
        return CloseWithResult(DialogResultType.Ok);
    }

    public Task Cancel()
    {
        return CloseWithResult(DialogResultType.Cancel);
    }
}
