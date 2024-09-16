using System.Diagnostics;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

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
    public ImageItem(string itemPath, double posX, double posY, double scale)
    {
        ItemPath = itemPath;
        PosX = posX;
        PosY = posY;
        Scale = scale;
    }

    public void SetSourceImage(BitmapImage bitmap)
    {
        _sourceImage = bitmap;
    }
    public void LoadItemSource()
    {
        if (string.IsNullOrEmpty(ItemPath)) return;

        try
        {
            _sourceImage = BitmapService.GetFromUri(ItemPath);

        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            //Could not load SourceImage from url ....
            throw;
        }
    }
}