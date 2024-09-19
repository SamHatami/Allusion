using Allusion.ViewModels.Dialogs;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Allusion.ViewModels;

public class MainViewModel : Conductor<object>, IHandle<NewRefBoardEvent>,
    IHandle<BoardIsModfiedEvent>, IHandle<BoardOpenedEvent>
{
    //TODO: Booleans on states -> enums

    public bool Saving;
    public bool Loading;


    
    private ImageViewModel _selectedImage;

    public BindableCollection<PageViewModel> Pages { get; set; } = [];
    
    public BindableCollection<ImageViewModel> Images { get; set; } = [];

    private List<ImageViewModel> _imageBin = [];

    private bool BoardIsModified; //TODO: Byt till BoardState 

    private ReferenceBoard _activeReferenceBoard;
    public ReferenceBoard ActiveReferenceBoard
    {
        get => _activeReferenceBoard;
        set { _activeReferenceBoard = value; NotifyOfPropertyChange(nameof(ActiveRefBoardName)); }
    }

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
                ActiveReferenceBoard.Name = ActiveRefBoardName;
        }
    }

    private string _text;
    public IReferenceBoardManager BoardManager { get; }
    public IEventAggregator Events { get; }
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
        IReferenceBoardManager refBoardManager)
    {
        _windowManager = windowManager;
        Events = events;
        Events.SubscribeOnBackgroundThread(this);
        BoardManager = refBoardManager;
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
        //replace with something else.
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

        await BoardManager.Save(_activeReferenceBoard);
        //needs to be sent from viewport so positions and scale can be transfered

        BoardIsModified = false;
    }

    private void InitializeRefBoard()
    {
        BoardIsModified = false;
        ActiveRefBoardName = BoardManager.CurrentRefBoard.Name;
        _activeBoardPage = BoardManager.CurrentRefBoard.Pages[0];
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
        Events.PublishOnBackgroundThreadAsync(new PasteOnCanvasEvent())
        var items = await BoardManager.GetPastedImageItems(0);

        AddImageItems(items);
    }




    public void Remove() //Key: Remove
    {
        _imageBin.Add(Images.Single(i => i == _selectedImage));

        Images.Remove(_selectedImage);
        BoardManager.RemoveImage(_selectedImage.Item);
    }

    public void UndoRemove() //Key Gesture: Ctrl-z
    {
        //TODO: Not done. Mind black out
        if (_imageBin.Count == 0) return;

        Images.Add(_imageBin.Last());

        _imageBin.RemoveAt(_imageBin.Count - 1);
    }



    public Task HandleAsync(NewRefBoardEvent message, CancellationToken cancellationToken)
    {
        BoardManager.CreateNew(message.Name);
        InitializeRefBoard();

        return Task.CompletedTask;
    }

    public Task HandleAsync(BoardIsModfiedEvent message, CancellationToken cancellationToken)
    {
        BoardIsModified = message.IsModfied;

        return Task.CompletedTask;
    }


    public Task HandleAsync(BoardOpenedEvent message, CancellationToken cancellationToken)
    {
        ActiveReferenceBoard = message.Board;
        InitializeRefBoard();

        return Task.CompletedTask;
    }

}