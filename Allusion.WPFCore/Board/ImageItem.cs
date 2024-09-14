using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Board;

[Serializable]
public class ImageItem : IItem
{
    public string ImageUri { get; set; }
    public double PosX { get; set; }
    public double PosY { get; set; }
    public double Scale { get; set; }
    public string Description { get; set; }
    public int MemberOfPage { get; set; }

    [JsonIgnore] public BitmapImage SourceImage { get; private set; }

    public ImageItem(string imageUri, double posX, double posY, double scale, int pagerNr, BitmapImage image)
    {
        ImageUri = imageUri;
        PosX = posX;
        PosY = posY;
        Scale = scale;
        MemberOfPage = pagerNr;
        SourceImage = image;
    }

    public void LoadItemSource()
    {
        if (string.IsNullOrEmpty(ImageUri)) return;

        try
        {
            SourceImage = BitmapService.GetFromUri(ImageUri);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            //Could not load SourceImage from url ....
            throw;
        }
    }
}