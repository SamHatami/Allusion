using System.IO;
using Caliburn.Micro;

namespace Allusion.ViewModels.Dialogs;

public class NewRefBoardViewModel : DialogScreen
{
    private string _prompt = "Name";
    private string _okText = "Ok";
    private string _newBoardName = string.Empty;
    private string _errorMessage = string.Empty;

    public string Prompt
    {
        get => _prompt;
        set
        {
            _prompt = value;
            NotifyOfPropertyChange(nameof(Prompt));
        }
    }

    public string OkText
    {
        get => _okText;
        set
        {
            _okText = value;
            NotifyOfPropertyChange(nameof(OkText));
        }
    }

    public string NewBoardName
    {
        get => _newBoardName;
        set
        {
            _newBoardName = value;
            NotifyOfPropertyChange(nameof(NewBoardName));
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            NotifyOfPropertyChange(nameof(ErrorMessage));
        }
    }

    public string ResultName { get; private set; } = string.Empty;

    public NewRefBoardViewModel(IEventAggregator events)
    {
        Title = "Create New Board";
    }

    public Task Ok()
    {
        var name = NewBoardName.Trim();
        if (string.IsNullOrEmpty(name))
        {
            ErrorMessage = "A board name is required.";
            return Task.CompletedTask;
        }

        if (name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            ErrorMessage = "Board names cannot contain path characters.";
            return Task.CompletedTask;
        }

        ErrorMessage = string.Empty;
        ResultName = name;
        return CloseWithResult(DialogResultType.Ok);
    }

    public Task Cancel()
    {
        return CloseWithResult(DialogResultType.Cancel);
    }
}
