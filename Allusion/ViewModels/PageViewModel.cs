using System.Drawing;
using System.Windows.Forms;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class PageViewModel : PropertyChangedBase, IHandle<NewImageItemsEvent>
{
    private readonly IPageManager _pageManager;
    private readonly IEventAggregator _events;

    private string _activePageName;

    public string ActivePageName
    {
        get => _activePageName;
        set
        {
            _activePageName = value;
            NotifyOfPropertyChange(nameof(ActivePageName));
        }
    }

    private BoardPage _activePage;

    public BoardPage ActivePage
    {
        get => _activePage;
        set
        {
            _activePage = value;
            NotifyOfPropertyChange(nameof(ActivePage));
        }
    }

    public BindableCollection<ImageViewModel> Images { get; set; }

    public PageViewModel(IPageManager pageManager, IEventAggregator events, BoardPage page)
    {
        _pageManager = pageManager;
        _events = events;
        _activePage = page;
        _events.SubscribeOnBackgroundThread(this);
        Images = new BindableCollection<ImageViewModel>();

        InitializePage();
    }

    private void InitializePage()
    {
        //BoardIsModified = false;

        foreach (var imageItem in _activePage.ImageItems)
            Images.Add(new ImageViewModel(imageItem, _events)
            {
                PosX = imageItem.PosX,
                PosY = imageItem.PosY
            });
    }

    private void AddItems(ImageItem[] items)
    {
        foreach (var item in items)
        {
            if (ActivePage.ImageItems.Contains(item)) continue; //override contains and isequal with a bitmap service comparor something

            Images.Add(new ImageViewModel(item, _events));
        }
    }

    public Task HandleAsync(NewImageItemsEvent message, CancellationToken cancellationToken)
    {
        AddItems(message.Items); //Todo: ..... haha...göra samma som nedan...

        return Task.CompletedTask;
    }
}