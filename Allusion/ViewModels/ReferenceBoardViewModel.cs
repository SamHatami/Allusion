using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class ReferenceBoardViewModel : PropertyChangedBase, IHandle<PageSelectedEvent>
{
    private ReferenceBoard _board;
    private readonly MainViewModel _parent;
    private readonly IEventAggregator _events;
    private readonly IReferenceBoardManager _boardManager;
    private readonly IPageManager _pageManager; 
    public BindableCollection<PageViewModel> Pages { get; private set; }

    public string BoardName { get; set; }

    public bool BoardIsSaved { get; set; }

    private IPageViewModel _activePageViewModel;

    public IPageViewModel ActivePageViewModel
    {
        get => _activePageViewModel;
        set
        {
            _activePageViewModel = value;
            NotifyOfPropertyChange(nameof(ActivePageViewModel));
        }
    }

    public ReferenceBoardViewModel(IEventAggregator events, IReferenceBoardManager boardManager, ReferenceBoard board, MainViewModel parent)
    {
        _board = board;
        _parent = parent;
        _events = events;
        _events.SubscribeOnBackgroundThread(this);
        _boardManager = boardManager;

        _pageManager = IoC.Get<IPageManager>();

        InitializeBoard();
    }

    private void InitializeBoard()
    {
        BoardName = _board.Name;
        Pages = [];
        if(_board.Pages is null) return;
        Pages.AddRange(_board.Pages.Select(p => new PageViewModel(_pageManager, _events, p, this)));
        NotifyOfPropertyChange(nameof(Pages));

        ActivePageViewModel = Pages[0];
        ActivePageViewModel.PageIsSelected = true;
    }

    public async Task Save()
    {
        foreach (var page in Pages)
        {
            page.TransferImageItems();
        }
        BoardIsSaved = await _boardManager.Save(_board);
        if(BoardIsSaved)
            _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(false));
        //needs to be sent from viewport so positions and scale can be transfered
    }

    public void AddPage(string pageName)
    {
        
        var newPage = _boardManager.AddPage(_board, pageName);

        Pages.Add(new PageViewModel(_pageManager,_events, newPage, this));

        ActivePageViewModel = Pages.Last();
        ActivePageViewModel.PageSelected();

        _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));
    }

    public Task HandleAsync(PageSelectedEvent message, CancellationToken cancellationToken)
    {
        ActivePageViewModel = message.Page;

        return Task.CompletedTask;
    }

    public async Task PasteOnCanvas()
    {
        await _parent.PasteOnCanvas();
    }

    public void RemoveSelectedImage()
    {
        ActivePageViewModel.DeleteSelectedImages();
    }

    public void RemovePage()
    {
        _boardManager.DeletePage(_board, ActivePageViewModel.Page);
        var currentPageIndex = Pages.IndexOf((PageViewModel)ActivePageViewModel);
        Pages.Remove((PageViewModel)ActivePageViewModel);

        if (Pages.Any())
        {
            ActivePageViewModel = Pages[currentPageIndex == 0 ? 0 : currentPageIndex - 1];
            ActivePageViewModel.PageSelected();
        }
        else
        {
            AddPage("");
        }

    }
    
}