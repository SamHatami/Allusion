using System.Drawing;
using System.Reflection.Metadata;
using System.Windows.Forms;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class PageViewModel : PropertyChangedBase , IRemovableItem, IItemOwner,IHandle<NewImageItemsEvent>, IHandle<ImageSelectionEvent>
{
    private readonly IPageManager _pageManager;
    private readonly IEventAggregator _events;

    private bool _isActive;

    private ImageViewModel _selectedImage;
    public ImageViewModel SelectedImage
    {
        get => _selectedImage;
        set { _selectedImage = value; NotifyOfPropertyChange(nameof(SelectedImage));}
    } //Bound to DependencyObject in View

    private string _displayName;
    public string DisplayName
    {
        get => _displayName;
        set
        {
            _displayName = value;
            NotifyOfPropertyChange(nameof(DisplayName));
        }
    }
    
    private BoardPage _page;
    public BoardPage Page
    {
        get => _page;
        set
        {
            _page = value;
            NotifyOfPropertyChange(nameof(Page));
        }
    }

    public BindableCollection<ImageViewModel> Images { get; set; }

    public PageViewModel(IPageManager pageManager, IEventAggregator events, BoardPage page)
    {
        _pageManager = pageManager;
        _events = events;
        _page = page;
        _events.SubscribeOnBackgroundThread(this);
        Images = new BindableCollection<ImageViewModel>();

        InitializePage();
    }


    private void InitializePage()
    {
        //BoardIsModified = false;
        if(_page.ImageItems is null) return;

        foreach (var imageItem in _page.ImageItems)
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
            if (Page.ImageItems.Contains(item)) 
                continue; //override contains and isequal with a bitmap service comparor something

            Images.Add(new ImageViewModel(item, _events));
            Page.ImageItems.Add(item);
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

    public void FitToView()
    {
        //Perhaps use https://github.com/ThomasMiz/RectpackSharp
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
        SelectedImage = message.ImageViewModel;

        //Deselect from hear instead of aggregating yet another event to all of them.
        foreach (var image in Images)
            if (image != _selectedImage)
                image.Selected = false;

        return Task.CompletedTask;
    }

    public void ReAddItem(IRemovableItem item) // Used by the sessionmanager
    {
        if(item is ImageViewModel image)
            Images.Add(image);
    }

}