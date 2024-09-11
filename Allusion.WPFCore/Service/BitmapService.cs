using System.IO;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Service
{
    public class BitmapService
    {
        public static BitmapSource? GetFromUri(string? uriString)
        {
            BitmapSource bitmap = null;

            if (!File.Exists(uriString)) return null;

            try
            {
                bitmap = new BitmapImage(new Uri(uriString));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return bitmap;
        }

        public static BitmapSource[] GetFromUri(string[]? fileUriStrings)
        {
            if (fileUriStrings == null) return null;

            List<BitmapSource> bitmaps = [];
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

        private static BitmapImage LoadFromUri(string uri)
        {
            return new BitmapImage(new Uri(uri));
        }

        public static async Task<BitmapSource>? DownloadAndConvert(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            using var client = new HttpClient();

            var bytes = await client.GetByteArrayAsync(url);

            var bitmap = CreateFromBytes(bytes);
            // Create a MemoryStream from the byte array

            return bitmap;
        }

        private static void SaveToFile(BitmapSource bitmap)
        {
            using (var fileStream = new FileStream(@"C:\Temp\" + "1.png", FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fileStream);
            }
        }

        //https://stackoverflow.com/questions/14337071/convert-array-of-bytes-to-bitmapimage
        private static BitmapSource? CreateFromBytes(byte[] imageBytes)
        {
            //using var stream = new MemoryStream(imageBytes);
            //var bitmap = new BitmapImage();
            //bitmap.BeginInit();
            //bitmap.CacheOption = BitmapCacheOption.OnLoad;
            //bitmap.StreamSource = stream;
            //bitmap.EndInit();

            //return bitmap;
            var bitmapSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(imageBytes);

            return bitmapSource;
        }
    }
}