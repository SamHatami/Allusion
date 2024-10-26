using Allusion.WPFCore.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Extensions;

//Taken from
//https://stackoverflow.com/questions/15558107/quickest-way-to-compare-two-bitmapimages-to-check-if-they-are-different-in-wpf
public static class BitmapImageExtensions
{
    public static bool IsEqual(this BitmapImage image1, BitmapImage image2)
    {
        if (image1 == null || image2 == null) return false;
        return image1.ToBytes().SequenceEqual(image2.ToBytes());
    }

    public static byte[] ToBytes(this BitmapImage image)
    {
        var data = new byte[] { };

        try
        {
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            return data;
        }
        catch (Exception e)
        {
            StaticLogger.Error("Error encoding image");
            StaticLogger.WriteToLog(e.Message, StaticLogger.LogLevel.Error);
        }

        return data;
    }

    public static BitmapImage? ConvertToBitmapImage(this BitmapSource source)
    {
        var bitmapImage = new BitmapImage();

        if (source is null) return bitmapImage;

        using var memoryStream = new MemoryStream();
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

        return bitmapImage;
    }
}