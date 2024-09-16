using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Allusion.ViewModels;

public class MainViewModel : Conductor<object>, IHandle<NewImageDropsEvent>, IHandle<NewRefBoardEvent>,
    IHandle<BoardIsModfiedEvent>, IHandle<ImageSelectedEvent>
{
    //TODO: Booleans on states -> enums

    public bool Saving;
    public bool Loading;

    private ImageViewModel _selectedImage;
    public BindableCollection<ImageViewModel> Images { get; set; } = [];

    private List<ImageViewModel> _imageBin = [];

    private bool BoardIsModified;

    private BoardPage _activeBoardPage;

    public BoardPage ActiveBoardPage
    {
        get => _activeBoardPage;
        set
        {
            _activeBoardPage = value;
            NotifyOfPropertyChange(nameof(ActiveBoardPage));
        }
    }

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

    public async Task NewRefBoardDialog()
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

    public async Task OpenRefBoardDialog()
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

        await BoardHandler.SaveRefBoard(imageItems);
        //needs to be sent from viewport so positions and scale can be transfered

        BoardIsModified = false;
    }

    private void InitializeRefBoard()
    {
        BoardIsModified = false;
        ActiveRefBoardName = BoardHandler.CurrentRefBoard.Name;
        _activeBoardPage = BoardHandler.CurrentRefBoard.Pages[0];
        Images.Clear();

        foreach (var imageItem in _activeBoardPage.ImageItems)
            Images.Add(new ImageViewModel(imageItem, _events)
            {
                PosX = imageItem.PosX,
                PosY = imageItem.PosY
            });
    }

    public async Task PasteOnCanvas()
    {
        var items = await BoardHandler.GetPastedImageItems(0);

        AddImageItems(items);
    }

    private void AddImageItems(ImageItem[] items)
    {
        foreach (var item in items)
        {
            Images.Add(new ImageViewModel(item, _events));
            BoardHandler.AddImage(item, _activeBoardPage);
        }

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

    public void Delete() //Key: Delete
    {
        _imageBin.Add(Images.Single(i => i == _selectedImage));

        Images.Remove(_selectedImage);
        BoardHandler.RemoveImage(_selectedImage.Item);
    }

    public void UndoRemove() //Key Gesture: Ctrl-z
    {
        //TODO: Not done. Mind black out
        if (_imageBin.Count == 0) return;

        Images.Add(_imageBin.Last());
        
        _imageBin.RemoveAt(_imageBin.Count - 1);
    }

    public Task HandleAsync(NewImageDropsEvent message, CancellationToken cancellationToken)
    {
        AddImageItems(message.DroppedItems);

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

    public Task HandleAsync(ImageSelectedEvent message, CancellationToken cancellationToken)
    {
        _selectedImage = message.ImageViewModel;

        //Deselect from hear instead of aggregating yet another event to all of them.
        foreach (var image in Images)
            if (image != _selectedImage)
                image.Selected = false;

        return Task.CompletedTask;
    }

    public void DeselectAll()
    {
        foreach (var image in Images)
            image.Selected = false;
    }
}