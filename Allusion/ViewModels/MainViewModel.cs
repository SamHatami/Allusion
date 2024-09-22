using Allusion.ViewModels.Dialogs;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Managers;
using Caliburn.Micro;
using System.Diagnostics;
using System.Windows;
using Allusion.Views;
using Allusion.WPFCore;

namespace Allusion.ViewModels;

public class MainViewModel : Conductor<object>, IHandle<NewRefBoardEvent>,
    IHandle<BoardIsModfiedEvent>, IHandle<BoardOpenedEvent>
{
    //TODO: Booleans on states -> enums

    public bool Saving;
    public bool Loading;

    private bool BoardIsModified; //TODO: Byt till BoardState

    private ReferenceBoard _currentRefBoard { get; set; }

    private ReferenceBoardViewModel? _refBoardViewModel;
    public ReferenceBoardViewModel? RefBoardViewModel
    {
        get => _refBoardViewModel;
        set
        {
            _refBoardViewModel = value;
            NotifyOfPropertyChange(nameof(RefBoardViewModel));
        }
    }

    private readonly IEventAggregator _events;//
    private readonly IReferenceBoardManager _boardManager;
    private readonly IWindowManager _windowManager;
    private readonly AllusionConfiguration _configuration;
    private Size _windowSize;

    public MainViewModel(IWindowManager windowManager, IEventAggregator events,
        IReferenceBoardManager refBoardManager, AllusionConfiguration configuration)
    {
        _windowManager = windowManager;
        _configuration = configuration;
        _events = events;
        _events.SubscribeOnBackgroundThread(this);
        _boardManager = refBoardManager;
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
                    await _refBoardViewModel.Save();
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
                    await _refBoardViewModel.Save();
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
        await _windowManager.ShowDialogAsync(IoC.Get<OpenRefBoardViewModel>());
    }

    public async Task PasteOnCanvas()
    {
        
        await _events.PublishOnBackgroundThreadAsync(new PasteOnCanvasEvent(_windowSize));
    }

    public void Remove() //Key: Remove
    {
        //DeleteEvent -> any pageviewmodel who has the state active + selected image will remove that
        RefBoardViewModel?.RemoveSelectedImage();
    }

    public void RemovePage()
    {
            RefBoardViewModel?.RemoveActivePage();
    }

    public void UndoRemove() //Key Gesture: Ctrl-z
    {

    }

    public async Task Save()
    {
        if(RefBoardViewModel is null) return;
        await RefBoardViewModel.Save();
    }
    public Task HandleAsync(NewRefBoardEvent message, CancellationToken cancellationToken)
    {
        _currentRefBoard = _boardManager.CreateNew(message.Name);
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
        _currentRefBoard = message.Board;
        InitializeRefBoard();

        return Task.CompletedTask;
    }

    private void InitializeRefBoard()
    {
        BoardIsModified = false;
        Debug.Assert(_boardManager is not null, "Holup");
        RefBoardViewModel = new ReferenceBoardViewModel(_events, _boardManager, _currentRefBoard);
    }

    protected override void OnViewLoaded(object view)
    {
        base.OnViewLoaded(view);

        var currentWindow = view as Window;

        if (currentWindow != null)
        {
            _windowSize = currentWindow.RenderSize;

            currentWindow.SizeChanged += OnCurrentWindowSizeChange;
        }
    }

    private void OnCurrentWindowSizeChange(object sender, SizeChangedEventArgs e)
    {
        _windowSize = e.NewSize;
    }
}