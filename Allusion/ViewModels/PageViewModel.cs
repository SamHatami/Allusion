using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Allusion.Events;
using Allusion.Views;
using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using Allusion.WPFCore.ValidationRules;
using Caliburn.Micro;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Application = System.Windows.Application;
using Screen = Caliburn.Micro.Screen;
using Size = System.Windows.Size;

namespace Allusion.ViewModels;

public class PageViewModel : Screen, IPageViewModel, IRemovableItem, IItemOwner, IHandle<NewImageItemsEvent>,
    IHandle<DropOnTabEvent>,
    IHandle<PageSelectedEvent>, IHandle<SelectionEvent>
{
    private readonly IPageManager _pageManager;
    private readonly IEventAggregator _events;
    public ReferenceBoardViewModel Board { get; }


    public List<ImageViewModel> SelectedImages { get; } = [];

    public BindableCollection<ImageViewModel> Images { get; set; }


    public IEnumerable<PageViewModel> OtherPages
    {
        get { return Board?.Pages.Where(p => p != this) ?? Enumerable.Empty<PageViewModel>(); }
    }


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

    //private ImageViewModel _selectedImage;

    //public ImageViewModel SelectedImage
    //{
    //    get => _selectedImage;
    //    set
    //    {
    //        _selectedImage = value;
    //        NotifyOfPropertyChange(nameof(SelectedImage));
    //    }
    //} //Bound to DependencyObject in View

    private string _displayName;

    public new string DisplayName
    {
        get => _displayName;
        set
        {
            if (!string.IsNullOrEmpty(value) && value != _displayName) 
            {
                _displayName = value.Trim();
                NotifyOfPropertyChange(nameof(DisplayName));
                _pageManager.RenamePage(Page, DisplayName);
            }
        }
    }

    public BoardPage Page { get; }

    private bool _isSelected;
    private readonly IWindowManager _windowManger;
    private Size _windowSize;

    public bool PageIsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            AllowDrop = !_isSelected;
            NotifyOfPropertyChange(nameof(PageIsSelected));
        }
    }


    private bool _allowDrop;

    public bool AllowDrop
    {
        get => _allowDrop;
        set
        {
            _allowDrop = value;
            NotifyOfPropertyChange(nameof(AllowDrop));
        }
    }


    public PageViewModel(IPageManager pageManager, IEventAggregator events, BoardPage page,
        ReferenceBoardViewModel board)
    {
        _pageManager = pageManager;
        _events = events;
        Board = board;
        Page = page;
        DisplayName = Page.Name;
        _events.SubscribeOnBackgroundThread(this);
        _events.SubscribeOnUIThread(this);
        Images = new BindableCollection<ImageViewModel>();
        _windowManger = IoC.Get<IWindowManager>();
        Images.CollectionChanged += (sender, args) => UpdateInfoBool();

        InitializePage();
    }

    private void InitializePage()
    {
        AllowDrop = !_isSelected;
        //Remove any imageitems without valid files.
        _pageManager.CleanPage(Page);
        //BoardIsModified = false;
        if (Page.ImageItems is null) return;

        foreach (var imageItem in Page.ImageItems)
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
            if (Page.ImageItems.Contains(item))
                continue; //override contains and isequal with a bitmap service comparor something

            Images.Add(new ImageViewModel(item, _events));
            _pageManager.AddImage(item, Page);
            itemsAdded = true;
        }

        if (itemsAdded)
            _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));
    }

    public async Task PasteOnCanvas()
    {
        await Board.PasteOnCanvas();
    }

    public async Task Save()
    {
        await Board.Save();
    }

    public void RemovePage()
    {
        Board.RemovePage();
    }

    public void MoveImage(PageViewModel targetPage)
    {
        foreach (var image in SelectedImages)
        {
            _pageManager.AddImage(image.Item, targetPage.Page);
            _pageManager.RemoveImage(image.Item, this.Page);

            Images.Remove(image);
            targetPage.Images.Add(image);
        }

        Images.Refresh();
        SelectedImages.Clear();

    }

    public void FitToView()
    {
        //Perhaps use https://github.com/ThomasMiz/RectpackSharp
    }

    public Task HandleAsync(NewImageItemsEvent message, CancellationToken cancellationToken)
    {
        if (!PageIsSelected) return Task.CompletedTask;

        if (Application.Current.Dispatcher.CheckAccess())
            AddItems(message.Items);
        else
            Application.Current.Dispatcher.Invoke(() => { AddItems(message.Items); });

        return Task.CompletedTask;
    }

    //public Task HandleAsync(ImageSelectionEvent message, CancellationToken cancellationToken)
    //{


    //    SelectedImage = message.ImageViewModel;

    //    //Deselect from hear instead of aggregating yet another event to all of them.
    //    foreach (var image in Images)
    //        if (image != _selectedImage)
    //            image.IsSelected = false;

    //    return Task.CompletedTask;
    //}

    private void ClearSelection()
    {
        foreach (var image in Images) image.IsSelected = false;
        SelectedImages.Clear();
    }

    public void ReAddItem(IRemovableItem item) // Used by the sessionmanager
    {
        if (item is ImageViewModel image)
            Images.Add(image);
    }

    public void TransferImageItems()
    {
        Page.ImageItems = Images.Select(i => i.Item).ToList();
    }

    public void DeleteSelectedImages()
    {
        var selectedImages = Images.Where(item => item.IsSelected).ToList();
        foreach (var image in selectedImages)
        {
            Images.Remove(image);
            _pageManager.RemoveImage(image.Item, Page);
        }
    }

    public void PageSelected()
    {
        PageIsSelected = true;
        _events.PublishOnBackgroundThreadAsync(new PageSelectedEvent(this));
    }

    public Task HandleAsync(PageSelectedEvent message, CancellationToken cancellationToken)
    {
        if (this != message.Page)
        {
            PageIsSelected = false;
            ClearSelection();
        }

        return Task.CompletedTask;
    }

    public Task HandleAsync(DropOnTabEvent message, CancellationToken cancellationToken)
    {
        if ((PageViewModel)message.TargetPage == this && message.ImageVM is ImageViewModel[] images)
            foreach (var image in images)
            {
                image.PosX = new Random().NextDouble() * 50 + 10;
                image.PosY = new Random().NextDouble() * 50 + 10;
                image.Dropped = false;
                image.IsSelected = false;
                Images.Add(image);
                _pageManager.AddImage(image.Item, Page);
                ClearSelection();
            }
        else if (Images.Intersect(message.ImageVM as ImageViewModel[]).Any())
            Images.RemoveRange(message.ImageVM as ImageViewModel[]);

        return Task.CompletedTask;
    }

    public void SetSingleSelection(ImageViewModel image)
    {
        if (SelectedImages.Contains(image)) return;

        ClearSelection();
        image.IsSelected = true;
        SelectedImages.Add(image);
    }

    public Task HandleAsync(SelectionEvent message, CancellationToken cancellationToken)
    {
        if (message.Images == null)
        {
            ClearSelection();
        }
        else
        {
            ClearSelection();

            //Framtida ctrl-click val
            switch (message.Type)
            {
                case SelectionType.DeSelect:
                    foreach (var image in Images)
                        image.IsSelected = false;
                    break;

                case SelectionType.Multi:
                    //SelectedImages.AddRange(message.Images);
                    //foreach (var image in message.Images)
                    //    image.IsSelected = true;
                    break;

                case SelectionType.Single:
                    if (SelectedImages.Intersect(message.Images).Any())
                        return Task.CompletedTask;
                    break;
            }
            
            foreach (var image in message.Images)
                image.IsSelected = true;
            SelectedImages.AddRange(message.Images);
        }


        return Task.CompletedTask;
    }

    public void AddToSelection(ImageViewModel imageViewModel)
    {
        SelectedImages.Add(imageViewModel);
        imageViewModel.IsSelected = true;
    }


    public void OpenPageFolder()
    {
        _pageManager.OpenPageFolder(Page);
    }
}