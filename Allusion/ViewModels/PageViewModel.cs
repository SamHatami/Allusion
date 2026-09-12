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
using Allusion.ViewModels.Arrangement;
using Allusion.ViewModels.Dialogs;
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
    private readonly ArrangeImageLayoutService _arrangeService = new();
    private readonly Stack<IReadOnlyList<ImageArrangeSnapshot>> _arrangeUndo = new();
    private ArrangeImageLayoutOptions _lastArrangeOptions = new();
    public ReferenceBoardViewModel Board { get; }


    public List<ImageViewModel> SelectedImages { get; } = [];

    public BindableCollection<ImageViewModel> Images { get; set; }

    public CanvasViewport Viewport { get; } = new();


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
            if (!string.IsNullOrEmpty(value.Trim()) && value.Trim() != _displayName) 
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

            Images.Add(new ImageViewModel(item, _events) { ZIndex = NextZIndex() });
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

    public async Task Arrange()
    {
        await ArrangeSettings();
    }

    public void ArrangeKeepCurrent() => ArrangeWithScaleMode(ArrangeScaleMode.KeepCurrent);

    public void ArrangeAverageHeight() => ArrangeWithScaleMode(ArrangeScaleMode.AverageHeight);

    public void ArrangeSmallestHeight() => ArrangeWithScaleMode(ArrangeScaleMode.SmallestHeight);

    private void ArrangeWithScaleMode(ArrangeScaleMode scaleMode)
    {
        ArrangeImages(GetDefaultArrangeImages(), _lastArrangeOptions with { ScaleMode = scaleMode });
    }

    public async Task ArrangeSettings()
    {
        var dialog = new ArrangeImagesViewModel(SelectedImages.Count > 0);
        var accepted = await _windowManger.ShowDialogAsync(dialog);

        if (accepted != true) return;

        ImageViewModel[] images = dialog.SelectedScope == ArrangeScope.SelectedImages
            ? SelectedImages.ToArray()
            : Images.ToArray();

        var options = dialog.CreateOptions();
        _lastArrangeOptions = options;
        ArrangeImages(images, options);
    }

    public bool CanUndoArrange => _arrangeUndo.Count > 0;

    public void UndoArrange()
    {
        if (_arrangeUndo.Count == 0) return;

        var snapshot = _arrangeUndo.Pop();
        foreach (var entry in snapshot)
        {
            entry.Image.Scale = entry.Scale;
            entry.Image.PosX = entry.X;
            entry.Image.PosY = entry.Y;
        }

        NotifyOfPropertyChange(nameof(CanUndoArrange));
        _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));
    }

    public void ZoomToExtent(Size viewportSize)
    {
        var bounds = GetImageBounds(Images);
        Viewport.FrameBounds(viewportSize, bounds);
    }

    public bool ScaleSelectedImages(double scaleFactor)
    {
        if (SelectedImages.Count == 0) return false;

        foreach (var image in SelectedImages)
            image.Scale *= scaleFactor;

        _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));
        return true;
    }

    public void BringToFront() => ApplyZOrder(ZOrderOperation.BringToFront);

    public void SendToBack() => ApplyZOrder(ZOrderOperation.SendToBack);

    public void BringForward() => ApplyZOrder(ZOrderOperation.BringForward);

    public void SendBackward() => ApplyZOrder(ZOrderOperation.SendBackward);

    public void AlignLeft() => AlignSelected(AlignEdge.Left);

    public void AlignRight() => AlignSelected(AlignEdge.Right);

    public void AlignTop() => AlignSelected(AlignEdge.Top);

    public void AlignBottom() => AlignSelected(AlignEdge.Bottom);

    public void AlignHorizontalCenters() => AlignSelected(AlignEdge.HorizontalCenters);

    public void AlignVerticalCenters() => AlignSelected(AlignEdge.VerticalCenters);

    private void ApplyZOrder(ZOrderOperation operation)
    {
        if (SelectedImages.Count == 0) return;

        var selected = new HashSet<ImageViewModel>(SelectedImages);
        var reordered = ImageZOrderService.Reorder(GetZOrderStack(), selected, operation);

        for (var i = 0; i < reordered.Count; i++)
            reordered[i].ZIndex = i;

        _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));
    }

    private List<ImageViewModel> GetZOrderStack()
    {
        // Ties fall back to collection order, which is how boards saved before
        // ZIndex existed were stacked.
        return Images
            .Select((image, index) => (image, index))
            .OrderBy(pair => pair.image.ZIndex)
            .ThenBy(pair => pair.index)
            .Select(pair => pair.image)
            .ToList();
    }

    private void AlignSelected(AlignEdge edge)
    {
        var images = SelectedImages.ToArray();
        if (images.Length < 2) return;

        var items = images
            .Select(image => new ImageAlignItem(image.PosX, image.PosY, image.Width, image.Height))
            .ToArray();
        var results = ImageAlignmentService.Align(items, edge);

        for (var i = 0; i < images.Length; i++)
        {
            images[i].PosX = results[i].X;
            images[i].PosY = results[i].Y;
        }

        _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));
    }

    private int NextZIndex()
    {
        return Images.Count == 0 ? 0 : Images.Max(image => image.ZIndex) + 1;
    }

    private ImageViewModel[] GetDefaultArrangeImages()
    {
        var source = SelectedImages.Count > 0
            ? SelectedImages.ToArray()
            : Images.ToArray();
        return SortForArrange(source);
    }

    private static ImageViewModel[] SortForArrange(IReadOnlyList<ImageViewModel> images)
    {
        return images.OrderBy(image => image.PosY).ThenBy(image => image.PosX).ToArray();
    }

    private void ArrangeImages(IReadOnlyList<ImageViewModel> images, ArrangeImageLayoutOptions options)
    {
        if (images.Count == 0) return;

        var ordered = SortForArrange(images);
        _arrangeUndo.Push(ordered.Select(image => new ImageArrangeSnapshot(image, image.PosX, image.PosY, image.Scale)).ToArray());
        NotifyOfPropertyChange(nameof(CanUndoArrange));

        var originX = CanvasGridSnap.Snap(ordered.Min(image => image.PosX));
        var originY = CanvasGridSnap.Snap(ordered.Min(image => image.PosY));
        var layoutItems = ordered.Select(BuildLayoutItem).ToArray();
        var results = _arrangeService.Arrange(layoutItems, options);

        for (var i = 0; i < ordered.Length; i++)
        {
            ordered[i].Scale = results[i].Scale;
            ordered[i].PosX = originX + results[i].X;
            ordered[i].PosY = originY + results[i].Y;
        }

        _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));
    }

    private static ArrangeImageLayoutItem BuildLayoutItem(ImageViewModel image)
    {
        var hasNote = !string.IsNullOrWhiteSpace(image.Description);
        return new ArrangeImageLayoutItem(
            Math.Max(0, image.Width),
            Math.Max(0, image.Height),
            image.Scale,
            hasNote ? image.DescriptorHeight : 0);
    }

    private static Rect GetImageBounds(IEnumerable<ImageViewModel> images)
    {
        var imageArray = images.ToArray();
        if (imageArray.Length == 0) return Rect.Empty;

        var left = imageArray.Min(image => image.PosX);
        var top = imageArray.Min(image => image.PosY);
        var right = imageArray.Max(image => image.PosX + image.Width);
        var bottom = imageArray.Max(image => image.PosY + image.Height);

        return new Rect(left, top, right - left, bottom - top);
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
                image.ZIndex = NextZIndex();
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
        ClearSelection();

        if (message.Images == null) return Task.CompletedTask;

        foreach (var image in message.Images)
            image.IsSelected = true;

        SelectedImages.AddRange(message.Images);

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

internal sealed record ImageArrangeSnapshot(ImageViewModel Image, double X, double Y, double Scale);
