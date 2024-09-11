using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Service;
using IDataObject = System.Windows.IDataObject;
using HtmlAgilityPack;

namespace Allusion.WPFCore.Extensions;

public static class DataObjectExtension
{
    public static async Task<BitmapSource>? GetBitmapAsync(this IDataObject dataObject)
    {
        if (dataObject.GetDataPresent(DataFormats.Bitmap))
        {
            if (dataObject.GetData(DataFormats.Bitmap) is BitmapSource bitmap)
            {
                return bitmap;
            }
        }

        // Check if the data contains HTML (could include the image URL)
        else if (dataObject.GetDataPresent(DataFormats.Html))
        {
            var htmlData = dataObject.GetData(DataFormats.Html) as string;

            if (string.IsNullOrEmpty(htmlData)) return null;

            var imageUrl = HtmlTest(htmlData);
            var bitmap = await BitmapService.DownloadAndConvert(imageUrl);

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

    //Note: For future reference regarading transparent channels.
    //https://stackoverflow.com/questions/44177115/copying-from-and-to-clipboard-loses-image-transparency?noredirect=1&lq=1

    //Note2: HTMLAgilityPack might be of use...

    private static string HtmlTest(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var node = htmlDoc.DocumentNode.SelectSingleNode("//img");
        var imageSource = node.Attributes["src"].Value;

        return imageSource;
    }
}