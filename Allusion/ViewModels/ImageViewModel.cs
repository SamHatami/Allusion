using System.Windows.Media;
using Caliburn.Micro;

namespace Allusion.ViewModels;

public class ImageViewModel : Screen
{
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

    private string _description = string.Empty;
    private double _posX;
    private double _posY;
    private bool _selected;
    public double Scale;
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

    public string Description
    {
        get => _description;
        set
        {
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

    public double PosX //kind of anti-pattern.
    {
        get => _posX;
        set
        {
            _posX = value;
            NotifyOfPropertyChange(nameof(PosX));
        }
    }

    public double PosY
    {
        get => _posY;
        set
        {
            _posY = value;
            NotifyOfPropertyChange(nameof(PosY));
        }
    }

    public ImageViewModel(ImageSource imageSource)
    {
        _imageSource = imageSource;
        _posX = 10;
        _posY = 40;

        
    }
}