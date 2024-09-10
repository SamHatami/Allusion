using System.IO;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Helpers;

public static class BitmapHelper
{
    public static BitmapSource[] GetImagesFromUri(string[]? fileUriStrings)
    {
        if (fileUriStrings == null) return null;

        List<BitmapSource> bitmaps = [];
        foreach (var file in fileUriStrings)
        {
            if (!File.Exists(file)) continue;

            try
            {
                //var bitmap = await Task.Run(() => LoadImage(file));
                bitmaps.Add(new BitmapImage(new Uri(file)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return bitmaps.ToArray();
    }

    public static BitmapSource? GetImageFromUri(string? uriString)
    {

        BitmapSource bitmap = null;

            if (!File.Exists(uriString)) return null;

            try
            {
                //var bitmap = await Task.Run(() => LoadImage(file));
                bitmap = new BitmapImage(new Uri(uriString));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            return bitmap;
    }

    private static BitmapImage LoadImage(string uri)
    {
        return new BitmapImage(new Uri(uri));
    }
}