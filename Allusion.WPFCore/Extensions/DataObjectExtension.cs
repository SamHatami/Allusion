using System.Runtime.CompilerServices;
using Allusion.WPFCore.Service;
using HtmlAgilityPack;
using System.Windows;
using System.Windows.Media.Imaging;
using IDataObject = System.Windows.IDataObject;

namespace Allusion.WPFCore.Extensions;

public static class DataObjectExtension
{
    public static async Task<BitmapSource>? GetBitmapAsync(this IDataObject dataObject)
    {
        if (dataObject.GetDataPresent(DataFormats.Bitmap))
        {
            if (dataObject.GetData(DataFormats.Bitmap) is BitmapSource bitmap) return bitmap;
        }

        // Check if the data contains HTML (could include the image URL)
        else if (dataObject.GetDataPresent(DataFormats.Html))
        {
            BitmapSource bitmap = null;
            if(!TryGetUrl(dataObject, out string imageUrl))
            {bitmap = await BitmapService.DownloadAndConvert(imageUrl);}

            return bitmap;
        }
        // Check if the data contains plain text. Need to check if its a valid url
        else if (dataObject.GetDataPresent(DataFormats.Text))
        {
            var textData = dataObject.GetData(DataFormats.Text) as string;
            if (!string.IsNullOrEmpty(textData))
            {
                var bitmap = await BitmapService.DownloadAndConvert(textData);
                // You could handle the URL or plain text here

                return bitmap;
            }
        }

        return null;
    }
    public static bool TryGetUrl(this IDataObject dataObject, out string url)
    {
        try
        {
            if (!dataObject.GetDataPresent(DataFormats.Html))
            {
                url = string.Empty;
                return false;
            }

            var htmlData = dataObject.GetData(DataFormats.Html) as string;

            if (string.IsNullOrEmpty(htmlData))
            {
                url = string.Empty;
                return false;
            }

            url = HtmlTest(htmlData);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static string HtmlTest(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var node = htmlDoc.DocumentNode.SelectSingleNode("//img");
        var imageSource = node.Attributes["src"].Value;

        return imageSource;
    }
}