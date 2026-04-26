namespace Allusion.ViewModels.Dialogs;

public class DialogViewModel : DialogScreen
{
    private readonly DialogType _type;

    public string Message { get; }
    public bool ShowOk { get; private set; }
    public bool ShowYes { get; private set; }
    public bool ShowNo { get; private set; }
    public bool ShowCancel { get; private set; }

    public DialogViewModel(string title, string message, DialogType type)
    {
        Title = title;
        Message = message;
        _type = type;

        SetButtons();
    }

    private void SetButtons()
    {
        switch (_type)
        {
            case DialogType.Choice:
                ShowYes = ShowNo = ShowCancel = true;
                break;

            case DialogType.Confirmation:
                ShowOk = ShowCancel = true;
                break;

            case DialogType.Information:
                ShowOk = true;
                break;
        }
    }

    public Task Ok()
    {
        return CloseWithResult(DialogResultType.Ok);
    }

    public Task No()
    {
        return CloseWithResult(DialogResultType.No);
    }

    public Task Yes()
    {
        return CloseWithResult(DialogResultType.Yes);
    }

    public Task Cancel()
    {
        return CloseWithResult(DialogResultType.Cancel);
    }
}

public enum DialogButton
{
    Ok,
    Cancel,
    Yes,
    No
}

public enum DialogType
{
    Confirmation,
    Choice,
    Information
}

public enum DialogResultType
{
    Yes,
    No,
    Cancel,
    Ok
}

public class DialogViewResult
{
    public DialogResultType DialogResult { get; }

    public DialogViewResult(DialogResultType dialogResult)
    {
        DialogResult = dialogResult;
    }
}
