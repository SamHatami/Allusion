using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Allusion.WPFCore.Service
{
    public class BitmapService
    {
        public static BitmapImage? GetFromUri(string? uriString)
        {
            BitmapImage bitmap = null;

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

        public static BitmapImage LoadFromUri(string uri)
        {
            return new BitmapImage(new Uri(uri));
        }

        BitmapImage LoadImageFromUri(string imageUri)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imageUri);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Optionally freeze for performance and cross-thread access
            return bitmapImage;
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

        public static BitmapImage ToBitmapImage(BitmapSource source)
        {
            BitmapImage bitmapImage = new BitmapImage();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Encode the BitmapSource to the stream
                PngBitmapEncoder encoder = new PngBitmapEncoder();
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
        //Note: This is not fully done, the scale will be wrong.
        private static BitmapSource? CreateFromBytes(byte[] imageBytes)
        {
            var bitmapSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(imageBytes);

            return bitmapSource;
        }
    }
}