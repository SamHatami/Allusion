using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Windows;
using Allusion.Views;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Caliburn.Micro;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Screen = Caliburn.Micro.Screen;
using Size = System.Windows.Size;

namespace Allusion.ViewModels;

public class PageViewModel : Screen, IPageViewModel, IRemovableItem, IItemOwner, IHandle<NewImageItemsEvent>,
    IHandle<ImageSelectionEvent>, IHandle<PageSelectedEvent>
{
    private readonly IPageManager _pageManager;
    private readonly IEventAggregator _events;
    private readonly ReferenceBoardViewModel _parent;

    public BindableCollection<ImageViewModel> Images { get; set; }


    private bool _showHelpBox;

    public bool ShowHelpBox
    {
        get => _showHelpBox;
        set
        {
            if (_showHelpBox == value) return;

            _showHelpBox = value;
            NotifyOfPropertyChange(nameof(ShowHelpBox));
        }
    }

    private ImageViewModel _selectedImage;

    public ImageViewModel SelectedImage
    {
        get => _selectedImage;
        set
        {
            _selectedImage = value;
            NotifyOfPropertyChange(nameof(SelectedImage));
        }
    } //Bound to DependencyObject in View

    private string _displayName;

    public new string DisplayName
    {
        get => _displayName;
        set
        {
            _displayName = value;
            NotifyOfPropertyChange(nameof(DisplayName));
            _pageManager.RenamePage(_page, DisplayName);
        }
    }

    private BoardPage _page { get; set; }

    private bool _isSelected;
    private readonly IWindowManager _windowManger;
    private Size _windowSize;

    public bool PageIsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            NotifyOfPropertyChange(nameof(PageIsSelected));

        }
    }



    public PageViewModel(IPageManager pageManager, IEventAggregator events, BoardPage page, ReferenceBoardViewModel parent)
    {
        _pageManager = pageManager;
        _events = events;
        _parent = parent;
        _page = page;
        DisplayName = _page.Name;
        _events.SubscribeOnBackgroundThread(this);
        _events.SubscribeOnUIThread(this);
        Images = new BindableCollection<ImageViewModel>();
        _windowManger = IoC.Get<IWindowManager>();
        Images.CollectionChanged += (sender, args) => UpdateInfoBool();



        InitializePage();
    }

    private void InitializePage()
    {
         //Remove any imageitems without valid files.
        _pageManager.CleanPage(_page);
        //BoardIsModified = false;
        if (_page.ImageItems is null) return;

        foreach (var imageItem in _page.ImageItems)
            Images.Add(new ImageViewModel(imageItem, _events)
            {
                PosX = imageItem.PosX,
                PosY = imageItem.PosY
            });

        UpdateInfoBool();

    }

    private void UpdateInfoBool()
    {
        ShowHelpBox = Images.Count == 0;
    }

    public void FocusImage(ImageViewModel image)
    {
        dynamic settings = new ExpandoObject();
        settings.WindowState = WindowState.Normal;
        _windowManger.ShowWindowAsync(new FocusViewModel(image.Item), null, settings);


    }

    private void AddItems(ImageItem[] items)
    {

        var itemsAdded = false;
        foreach (var item in items)
        {
            if (_page.ImageItems.Contains(item))
                continue; //override contains and isequal with a bitmap service comparor something

            Images.Add(new ImageViewModel(item, _events));
            _pageManager.AddImage(item, _page);
            itemsAdded = true;
        }

        if (itemsAdded)
            _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));
    }

    public async Task PasteOnCanvas()
    {

        await _parent.PasteOnCanvas();
    }

    public async Task Save()
    {
        await _parent.Save();
    }

    public void FitToView()
    {
        //Perhaps use https://github.com/ThomasMiz/RectpackSharp
    }

    public Task HandleAsync(NewImageItemsEvent message, CancellationToken cancellationToken)
    {
        if (!PageIsSelected) return Task.CompletedTask;

        if(Application.Current.Dispatcher.CheckAccess())
            AddItems(message.Items);
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddItems(message.Items);
            });
        }

        return Task.CompletedTask;
    }

    public Task HandleAsync(ImageSelectionEvent message, CancellationToken cancellationToken)
    {
        switch (message.Type)
        {
            case SelectionType.DeSelect:
                foreach (var image in Images)
                    image.IsSelected = false;
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
                image.IsSelected = false;

        return Task.CompletedTask;
    }

    private void DeSelectAllImages()
    {
        foreach (var image in Images) image.IsSelected = false;
    }

    public void ReAddItem(IRemovableItem item) // Used by the sessionmanager
    {
        if (item is ImageViewModel image)
            Images.Add(image);
    }

    public void TransferImageItems()
    {
        _page.ImageItems = Images.Select(i => i.Item).ToList();
    }

    public void DeleteSelectedImages()
    {
        var selectedImages = Images.Where(item => item.IsSelected).ToList();
        foreach (var image in selectedImages)
        {
            Images.Remove(image);
            _pageManager.RemoveImage(image.Item, _page);
        }
    }

    public void SelectPage()
    {
        PageIsSelected = true;
        _events.PublishOnBackgroundThreadAsync(new PageSelectedEvent(this));
    }

    public Task HandleAsync(PageSelectedEvent message, CancellationToken cancellationToken)
    {
        if (this != message.Page)
        {
            PageIsSelected = false;
            DeSelectAllImages();
        }

        return Task.CompletedTask;
    }

}