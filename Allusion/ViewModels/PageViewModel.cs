using System.Drawing;
using System.Reflection.Metadata;
using System.Windows.Forms;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class PageViewModel : PropertyChangedBase, IHandle<NewImageItemsEvent>, IHandle<ImageSelectionEvent>
{
    private readonly IPageManager _pageManager;
    private readonly IEventAggregator _events;

    private ImageViewModel _selectedImage;
    public ImageViewModel SelectedImage
    {
        get => _selectedImage;
        set { _selectedImage = value; NotifyOfPropertyChange(nameof(SelectedImage));}
    } //Bound to DependencyObject in View

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
    private List<ImageViewModel> _imageBin; //remove action temporary holds objects here

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
        var itemsAdded = false;
        foreach (var item in items)
        {
            if (ActivePage.ImageItems.Contains(item)) 
                continue; //override contains and isequal with a bitmap service comparor something

            Images.Add(new ImageViewModel(item, _events));
            itemsAdded = true;

        }
        if (itemsAdded)
            _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));
    }

    public void Remove()
    {

    }

    public void UndoRemove()
    {

    }

    public Task HandleAsync(NewImageItemsEvent message, CancellationToken cancellationToken)
    {
        AddItems(message.Items);

        return Task.CompletedTask;
    }

    public Task HandleAsync(ImageSelectionEvent message, CancellationToken cancellationToken)
    {
        switch (message.Type)
        {
            case SelectionType.DeSelect:
                foreach (var image in Images)
                    image.Selected = false;
                break;
            case SelectionType.Multi:
            //TODO:
                break;
            case SelectionType.Single:
                _selectedImage = message.ImageViewModel;
                break;
        }
        _selectedImage = message.ImageViewModel;

        //Deselect from hear instead of aggregating yet another event to all of them.
        foreach (var image in Images)
            if (image != _selectedImage)
                image.Selected = false;

        return Task.CompletedTask;
    }
}