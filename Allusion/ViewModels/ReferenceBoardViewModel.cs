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

    public ReferenceBoardViewModel(IEventAggregator events, IReferenceBoardManager boardManager, ReferenceBoard board)
    {
        _board = board;
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
        Pages.AddRange(_board.Pages.Select(p => new PageViewModel(_pageManager, _events, p)));

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

        Pages.Add(new PageViewModel(_pageManager,_events, newPage));

        ActivePageViewModel = Pages.Last();
        ActivePageViewModel.SelectPage();
    }

    public Task HandleAsync(PageSelectedEvent message, CancellationToken cancellationToken)
    {
        ActivePageViewModel = message.Page;

        return Task.CompletedTask;
    }

    public void RemoveSelectedImage()
    {
        ActivePageViewModel.DeleteSelectedImages();
    }

    public void RemoveActivePage()
    {
        
    }
}