using System.Diagnostics;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Utilities;

namespace Allusion.WPFCore.Board;

[Serializable]
public class ImageItem : IItem
{

    public string ItemPath { get; set; }
    public double PosX { get; set; }
    public double PosY { get; set; }
    public double Scale { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid MemberOfPage { get; set; } = Guid.Empty;


    [JsonIgnore] private readonly IBitmapService _bitmapService = new BitmapService();
    [JsonIgnore] private readonly ImageItemService _itemService;
    [JsonIgnore] private BitmapImage _sourceImage;
    [JsonIgnore] public BitmapImage SourceImage
    {
        get
        {
            if (_loaded) return _sourceImage; 

            LoadItemSource();
            _loaded = true;

            return _sourceImage;
        }
        private set => _sourceImage = value;
    }

    [JsonIgnore] private bool _loaded;

    [JsonConstructor]
    public ImageItem(double posX, double posY, double scale)
    {
        PosX = posX;
        PosY = posY;
        Scale = scale;
    }

    public void SetSourceImage(BitmapImage bitmap)
    {
        _sourceImage = bitmap;
        _loaded = true;
    }
    public void LoadItemSource()
    {
        if (string.IsNullOrEmpty(ItemPath)) return;

        try
        {
            _sourceImage = _bitmapService.GetFromUri(ItemPath) ?? BitmapUtils.DefaultImage();

        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            //Could not load SourceImage from url ....
            throw;
        }
    }
}