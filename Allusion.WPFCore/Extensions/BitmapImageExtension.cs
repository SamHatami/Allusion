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
        if (image != null)
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
            catch (Exception ex)
            {
            }

        return data;
    }
}