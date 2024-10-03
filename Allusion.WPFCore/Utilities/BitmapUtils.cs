using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Utilities;

public static class BitmapUtils
{
    public static void SaveToFile(BitmapImage bitmap, string fullFileNameWithoutExtension)
    {
        var fileNamePNG = fullFileNameWithoutExtension + ".png";
        using var fileStream = new FileStream(fullFileNameWithoutExtension, FileMode.Create);
        BitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(fileStream);
    }

    public static BitmapImage LoadImageFromUri(string imageUri)
    {
        var bitmapImage = new BitmapImage();

        try
        {
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imageUri);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Optionally freeze for performance and cross-thread access
        }
        catch (UriFormatException e)
        {
            Console.WriteLine(e);
            throw;
        }

        return bitmapImage;
    }

    //https://stackoverflow.com/questions/14337071/convert-array-of-bytes-to-bitmapimage
    //Note: This is not fully done, the scale will be wrong.
    public static BitmapSource? CreateFromBytes(byte[] imageBytes)
    {
        BitmapSource? bitmapSource = null;
        try
        {
            bitmapSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(imageBytes);

        }
        catch (NotSupportedException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (NullReferenceException e)
        {

        }

        return bitmapSource;
    }

    public static string GetUrl(BitmapImage? bitmapSource)
    {

        var url = string.Empty;
        if (bitmapSource is null) return url;

        try
        {
            if (bitmapSource.UriSource is not null) url = bitmapSource.UriSource.AbsolutePath;

            if (bitmapSource.BaseUri is not null) url = bitmapSource.BaseUri.AbsolutePath;
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            throw;
        }

        url = string.IsNullOrEmpty(url) ? "UnknownSource" : url;
        return url;
    }

    public static BitmapImage DefaultImage()
    {
        return new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultNoImage.png"));
    }
}