using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Caliburn.Micro;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Allusion.WPFCore.Interfaces;

namespace Allusion.ViewModels;

public class ImageViewModel : PropertyChangedBase, IRemovableItem
{
    private ImageItem _item;
    private readonly IEventAggregator _events;

    public double Width { get; private set; } //This is for initalizing height or width of pageview - grid control
    public double Height { get; private set; } //This is for initalizing height or width of pageview - grid control


    public ImageItem Item
    {
        get => TransferToItem();
        set => _item = value;
    }

    private ImageSource _imageSource;

    public ImageSource ImageSource
    {
        get => _imageSource;
        set
        {
            _imageSource = value;
            NotifyOfPropertyChange(nameof(ImageSource));
        }
    }

    public double Scale; // Is set code behind of thumb, probably not the best idea, not sure how to not do this.
    public double AspectRatio { get; set; }
    private double _descriptorHeight = 30.0;

    public double DescriptorHeight
    {
        get => _descriptorHeight;
        set
        {
            _descriptorHeight = value;
            NotifyOfPropertyChange(nameof(DescriptorHeight));
        }
    }

    private string _description = string.Empty;

    public string Description
    {
        get => _description;
        set
        {
            if (value != _description)
                _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));

            _description = value;
            NotifyOfPropertyChange(nameof(Description));
        }
    }
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            NotifyOfPropertyChange(nameof(IsSelected));
        }
    }

    private double _posX;

    public double PosX //kind of anti-pattern.
    {
        get => _posX;
        set
        {
            if (value != _posX)
                _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));

            _posX = value;
            NotifyOfPropertyChange(nameof(PosX));
        }
    }

    private double _posY;

    public double PosY
    {
        get => _posY;
        set
        {
            if (value != _posY)
                _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));

            _posY = value;
            NotifyOfPropertyChange(nameof(PosY));
        }
    }

    private int _pageMember;

    public int PageMember //TODO: Probably use a MoveToPageEvent. This comes from a user input
    {
        get => _pageMember;
        set
        {
            if (value != _pageMember)
                _events.PublishOnBackgroundThreadAsync(new BoardIsModfiedEvent(true));

            _pageMember = value;
            NotifyOfPropertyChange(nameof(PageMember));
        }
    }

    public ImageViewModel(ImageItem item, IEventAggregator events)
    {
        _item = item;
        _events = events;
        Initialize();
    }

    private void Initialize()
    {
        
        ImageSource = _item.SourceImage;
        _posX = _item.PosX;
        _posY = _item.PosY;
        Scale = _item.Scale > 0 ? _item.Scale : 1.0;
        Description = _item.Description;
        Height = _imageSource.Height * Scale;
        Width = _imageSource.Width * Scale;
        AspectRatio = _imageSource.Width / _imageSource.Height;
    }



    private ImageItem TransferToItem()
    {
        _item.Description = Description;
        _item.PosX = PosX;
        _item.PosY = PosY;
        _item.Scale = Scale;

        return _item;
    }

    public void ImageClick(ModifierKeys modifier)
    {
        //this comes from code-behind since I couldn't get the Modifier argument sent to the viewmodel from actions
        IsSelected = true;
        var multiSelect = (modifier & ModifierKeys.Control) == ModifierKeys.Control;
        _events.PublishOnUIThreadAsync(new ImageSelectionEvent(this, SelectionType.Multi));
    }
}

public class ImageSelectionEvent
{
    public ImageViewModel ImageViewModel { get; } //Don't want core to handle this....
    public SelectionType Type { get; }

    public ImageSelectionEvent(ImageViewModel imageViewModel, SelectionType type)
    {
        ImageViewModel = imageViewModel;
        Type = type;
    }
}

public enum SelectionType
{
    Single,
    Multi,
    DeSelect
}