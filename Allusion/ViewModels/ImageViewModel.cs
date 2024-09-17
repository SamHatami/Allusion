using Allusion.WPFCore.Board;
using Allusion.WPFCore.Events;
using Caliburn.Micro;
using System.Windows.Input;
using System.Windows.Media;

namespace Allusion.ViewModels;

public class ImageViewModel : Screen
{
    private ImageItem _item;
    private readonly IEventAggregator _events;

    public ImageItem Item
    {
        get => TransferToItem();
        set => _item = value;
    }

    private bool _selected;
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

    public double Scale;
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

    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            NotifyOfPropertyChange(nameof(Selected));
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

    public int PageMember
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
        Description = _item.Description;
        AspectRatio = _imageSource.Width / _imageSource.Height;
    }

    private ImageItem TransferToItem()
    {
        _item.Description = Description;
        _item.PosX = PosX;
        _item.PosY = PosY;

        return _item;
    }

    public void ImageClick(ModifierKeys modifier)
    {
        //this comes from code-behind since I couldn't get the Modifier argument sent to the viewmodel from actions
        Selected = true;
        var multiSelect = (modifier & ModifierKeys.Control) == ModifierKeys.Control;
        _events.PublishOnCurrentThreadAsync(new ImageSelectedEvent(this, multiSelect));
    }
}

public class ImageSelectedEvent
{
    public ImageViewModel ImageViewModel { get; }
    public bool MultiSelect { get; }

    public ImageSelectedEvent(ImageViewModel imageViewModel, bool multiSelect)
    {
        ImageViewModel = imageViewModel;
        MultiSelect = multiSelect;
    }
}