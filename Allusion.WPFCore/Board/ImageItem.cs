using System.Diagnostics;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Service;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Board;

[Serializable]
public class ImageItem : IItem
{
    public string ImagePath { get; set; }
    public double PosX { get; set; }
    public double PosY { get; set; }
    public double Scale { get; set; } = 1;
    public string Description { get; set; } = String.Empty;
    public int MemberOfPage { get; set; } = 0;

    [JsonIgnore] 
    public BitmapImage SourceImage { get; private set; }

    [JsonConstructor]
    public ImageItem(string imagePath, double posX, double posY, double scale, int memberOfPage)
    {
        ImagePath = imagePath;
        PosX = posX;
        PosY = posY;
        Scale = scale;
        MemberOfPage = memberOfPage;
    }

    public void SetSourceImage(BitmapImage bitmap)
    {
        SourceImage = bitmap;
    }
    public void LoadItemSource()
    {
        if (string.IsNullOrEmpty(ImagePath)) return;

        try
        {
            SourceImage = BitmapService.GetFromUri(ImagePath);

        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            //Could not load SourceImage from url ....
            throw;
        }
    }
}