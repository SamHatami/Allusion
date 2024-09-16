using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using System.Diagnostics;
using System.Windows;

namespace Allusion.ViewModels;

public class MainViewModel : Conductor<object>, IHandle<NewImageDropsEvent>, IHandle<NewRefBoardEvent>,
    IHandle<BoardIsModfiedEvent>
{
    //TODO: Booleans on states -> enums

    public bool Saving;
    public bool Loading;

    public BindableCollection<ImageViewModel> Images { get; set; } = [];

    private bool BoardIsModified;
    private string _activeRefBoardName;

    public string ActiveRefBoardName
    {
        get => _activeRefBoardName;
        set
        {
            _activeRefBoardName = value;
            NotifyOfPropertyChange(nameof(ActiveRefBoardName));
            if (SettingBoardName)
                BoardHandler.CurrentRefBoard.Name = ActiveRefBoardName;
        }
    }

    private string _text;
    public IReferenceBoardHandler BoardHandler { get; }
    private IEventAggregator _events { get; }
    private IWindowManager _windowManager;

    private bool _settingBoardName;

    public bool SettingBoardName
    {
        get => _settingBoardName;
        set
        {
            _settingBoardName = value;
            NotifyOfPropertyChange(nameof(SettingBoardName));
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            NotifyOfPropertyChange(nameof(Text));
        }
    }

    public MainViewModel(IWindowManager windowManager, IEventAggregator events,
        IReferenceBoardHandler RefBoardHandler)
    {
        _windowManager = windowManager;
        _events = events;
        _events.SubscribeOnBackgroundThread(this);
        BoardHandler = RefBoardHandler;
        SettingBoardName = false;
    }

    public void EditBoardName()
    {
        SettingBoardName = !_settingBoardName;
    }

    public void FitToView()
    {
        //Perhaps use https://github.com/ThomasMiz/RectpackSharp
    }

    private DialogResultType ShowBoardModifiedSaveDialog()
    {
        var dialog = new DialogViewModel("Save file ?",
            "The board has been modified since last save. Do you want to save before continuing?",
            DialogType.Choice);
        var option = _windowManager.ShowDialogAsync(dialog);

        return dialog.DialogResult;
    }

    public async Task NewRefBoard()
    {
        if (BoardIsModified)
        {
            var dialogResult = ShowBoardModifiedSaveDialog();

            switch (dialogResult)
            {
                case DialogResultType.Yes:
                    await SaveRefBoard();
                    ShowNewRefBoardDialog();
                    break;

                case DialogResultType.No:
                    ShowNewRefBoardDialog();
                    break;
            }
        }
        else
        {
            ShowNewRefBoardDialog();
        }
    }

    private void ShowNewRefBoardDialog()
    {
        _windowManager.ShowDialogAsync(IoC.Get<NewRefBoardViewModel>());
    }

    public async Task OpenRefBoard()
    {
        if (BoardIsModified)
        {
            var dialogResult = ShowBoardModifiedSaveDialog();

            switch (dialogResult)
            {
                case DialogResultType.Yes:
                    await SaveRefBoard();
                    await ShowOpenBoardDialog();
                    break;

                case DialogResultType.No:
                    await ShowOpenBoardDialog();
                    break;

                case DialogResultType.Cancel: return;
            }
        }
        else
        {
            await ShowOpenBoardDialog();
        }
    }

    private async Task ShowOpenBoardDialog()
    {
        var option = await _windowManager.ShowDialogAsync(IoC.Get<OpenRefBoardViewModel>()) ?? false;

        if (option) InitializeRefBoard();
    }

    public async Task SaveRefBoard()
    {
        var imageItems = Images.Select(i => i.Item).ToArray();

        await BoardHandler
            .SaveRefBoard(imageItems); //needs to be sent from viewport so positions and scale can be transfered

        BoardIsModified = false;
    }

    private void InitializeRefBoard()
    {
        BoardIsModified = false;
        ActiveRefBoardName = BoardHandler.CurrentRefBoard.Name;
        Images.Clear();

        foreach (var imageItem in BoardHandler.CurrentRefBoard.Images)
            Images.Add(new ImageViewModel(imageItem, _events)
            {
                PosX = imageItem.PosX,
                PosY = imageItem.PosY
            });
    }

    public async Task PasteOnCanvas()
    {
        var items = await BoardHandler.GetPastedImageItems(0);

        if (items == null) return;

        foreach (var item in items)
            Images.Add(new ImageViewModel(item, _events));

        BoardIsModified = true;
    }

    private void ExtractUrlFromDataObject()
    {
        var dataObject = Clipboard.GetDataObject();
        if (dataObject != null)
            foreach (var format in dataObject.GetFormats())
            {
                var data = dataObject.GetData(format);
                // Inspect data for potential URL or source information
                Debug.WriteLine($"Format: {format}, Data: {data}");
            }

        BoardIsModified = true;
    }

    public void Delete()
    {
        Text = "Pressed delete";
    }

    public Task HandleAsync(NewImageDropsEvent message, CancellationToken cancellationToken)
    {
        foreach (var item in message.DroppedItems)
            Images.Add(new ImageViewModel(item, _events));

        BoardIsModified = true;

        return Task.CompletedTask;
    }

    public Task HandleAsync(NewRefBoardEvent message, CancellationToken cancellationToken)
    {
        BoardHandler.CreateNewRefBoard(message.Name);
        InitializeRefBoard();

        return Task.CompletedTask;
    }

    public Task HandleAsync(BoardIsModfiedEvent message, CancellationToken cancellationToken)
    {
        BoardIsModified = message.IsModfied;

        return Task.CompletedTask;
    }
}