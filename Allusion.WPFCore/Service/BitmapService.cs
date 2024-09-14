using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Service;

public class BitmapService
{
    public static BitmapImage? GetFromUri(string? uriString)
    {
        BitmapImage bitmap = null;

        if (!File.Exists(uriString)) return null;

        if (Application.Current.Dispatcher.CheckAccess())
            return LoadImageFromUri(uriString);
        else
            return Application.Current.Dispatcher.Invoke(() => LoadImageFromUri(uriString));
    }

    public static BitmapImage[] LoadFromUri(string[]? fileUriStrings)
    {
        if (fileUriStrings == null) return null;

        List<BitmapImage> bitmaps = [];
        foreach (var file in fileUriStrings)
        {
            if (!File.Exists(file)) continue;

            try
            {
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

    public BitmapImage LoadFromUri(string uri)
    {
        return new BitmapImage(new Uri(uri));
    }

    public static string GetUrl(BitmapImage bitmapSource)
    {
        var url = string.Empty;
        try
        {
            if (bitmapSource.UriSource is not null) url = bitmapSource.UriSource.AbsolutePath;

            if (bitmapSource.BaseUri is not null) url = bitmapSource.BaseUri.AbsolutePath;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        url = "UnknownSource";
        return url;
    }

    private static BitmapImage LoadImageFromUri(string imageUri)
    {
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = new Uri(imageUri);
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze(); // Optionally freeze for performance and cross-thread access
        return bitmapImage;
    }

    public async Task<BitmapImage>? DownloadAndConvert(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        using var client = new HttpClient();

        BitmapSource bitmapSource = null;

        try
        {
            var bytes = await client.GetByteArrayAsync(url);
            bitmapSource = CreateFromBytes(bytes);
        }
        catch (Exception e)
        {
            return null;
        }

        // Create a MemoryStream from the byte array

        return ToBitmapImage(bitmapSource);
    }

    public BitmapImage ToBitmapImage(BitmapSource source)
    {
        var bitmapImage = new BitmapImage();

        if (source is null) return bitmapImage;

        using (var memoryStream = new MemoryStream())
        {
            // Encode the BitmapSource to the stream
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(memoryStream);

            // Rewind the stream
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Create BitmapImage and load the stream
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Optionally freeze to make it cross-thread accessible
        }

        return bitmapImage;
    }

    public static void SaveToFile(BitmapImage bitmap, string fullFileNameWithoutExtension)
    {
        var fileNamePNG = fullFileNameWithoutExtension + ".png";
        using (var fileStream = new FileStream(fullFileNameWithoutExtension, FileMode.Create))
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(fileStream);
        }
    }

    //https://stackoverflow.com/questions/14337071/convert-array-of-bytes-to-bitmapimage
    //Note: This is not fully done, the scale will be wrong.
    private static BitmapSource? CreateFromBytes(byte[] imageBytes)
    {
        var bitmapSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(imageBytes);

        return bitmapSource;
    }
}