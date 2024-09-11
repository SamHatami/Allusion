using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using IDataObject = System.Windows.IDataObject;

namespace Allusion.WPFCore.Extensions;

public static class DataObjectExtension
{
    //Most cred goes to ChatGPT
    public static BitmapSource GetBitmap(this IDataObject dataObject)
    {
        if (dataObject.GetDataPresent(DataFormats.Bitmap))
        {
            // Get the image
            var bitmap = dataObject.GetData(DataFormats.Bitmap) as BitmapSource;
            if (bitmap != null)
            {
                return bitmap;
            }
        }
        // Check if the data contains HTML (could include the image URL)
        else if (dataObject.GetDataPresent(DataFormats.Html))
        {
            var htmlData = dataObject.GetData(DataFormats.Html) as string;
            if (!string.IsNullOrEmpty(htmlData))
            {
                // Extract image URL or other information from HTML
                string imageUrl = ExtractImageUrlFromHtml(htmlData);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    using var client = new WebClient();

                    // Download the image data as a byte array
                    byte[] bytes = client.DownloadData(imageUrl);

                    // Create a MemoryStream from the byte array
                    using var stream = new MemoryStream(bytes);

                    // Create a BitmapImage from the stream
                    var bitmap = new BitmapImage();

                    // Initialize the BitmapImage
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Ensures that the stream can be closed after loading
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();

                    return bitmap;
                }
            }
        }
        // Check if the data contains plain text (URL or other text)
        //else if (dataObject.GetDataPresent(DataFormats.Text))
        //{
        //    var textData = dataObject.GetData(DataFormats.Text) as string;
        //    if (!string.IsNullOrEmpty(textData))
        //    {
        //        MessageBox.Show($"Dragged Text: {textData}");
        //        // You could handle the URL or plain text here
        //    }
        //}

        return null;
    }

    private static string ExtractImageUrlFromHtml(string html)
    {
        // Use regex or an HTML parser to extract the image URL
        // Example: match <img> tags or "src" attributes
        // Simplified regex for an image URL:
        var match = Regex.Match(html, @"<img[^>]*?src=[""']([^""']+)[""']");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    //Note: For future reference regarading transparent channels.
    //https://stackoverflow.com/questions/44177115/copying-from-and-to-clipboard-loses-image-transparency?noredirect=1&lq=1
}