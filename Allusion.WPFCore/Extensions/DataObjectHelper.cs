using System.Runtime.CompilerServices;
using Allusion.WPFCore.Service;
using HtmlAgilityPack;
using System.Windows;
using System.Windows.Media.Imaging;
using IDataObject = System.Windows.IDataObject;
using System.Diagnostics;
using System.IO;

namespace Allusion.WPFCore.Extensions;

public class DataObjectHelper
{
    private BitmapService _bitmapService = new();

    public BitmapSource[] GetBitmapFromLocal(IDataObject dataObject)
    {
        //This is just so crazy...
        List<BitmapSource> bitmaps = [];

        //Get formats for this dataobject as helper for conversion
        var formats = dataObject.GetFormats(true);
        if (formats == null || formats.Length == 0) return null;

        foreach (var f in formats)
            Debug.WriteLine(" - " + f.ToString());

        //Multiple file paste from explorer
        if (formats.Contains("FileContents"))
        {
            string[] filePaths = dataObject.GetData("FileContents") as string[];

            return BitmapService.LoadFromUri(filePaths);
        }

        //if the SourceImage was pasted from explorer
        if (formats.Contains("FileName"))
        {
            string[] filePaths = dataObject.GetData("FileName") as string[];

            return BitmapService.LoadFromUri(filePaths);
        }

        if (formats.Contains("PNG"))
        {
            Debug.WriteLine("PNG");

            using (var ms = (MemoryStream)dataObject.GetData("PNG"))
            {
                ms.Position = 0;
                //var bit = new Bitmap(ms);
            }
        }

        return null;
    } //This is handled by the clipboardService

    public async Task<BitmapImage> GetWebBitmapAsync(IDataObject dataObject)
    {
        if (dataObject.GetDataPresent(DataFormats.Bitmap))
        {
            if (dataObject.GetData(DataFormats.Bitmap) is BitmapImage bitmap) return bitmap;
        }

        // Check if the data contains HTML (could include the SourceImage URL)
        else if (dataObject.GetDataPresent(DataFormats.Html))
        {
            BitmapImage bitmap = null;
            if (!TryGetUrl(dataObject, out var imageUrl)) bitmap = await _bitmapService.DownloadAndConvert(imageUrl);

            return bitmap;
        }
        // Check if the data contains plain text. Need to check if its a valid url ?
        else if (dataObject.GetDataPresent(DataFormats.Text))
        {
            var textData = dataObject.GetData(DataFormats.Text) as string;
            if (string.IsNullOrEmpty(textData)) return null;

            var bitmap = await _bitmapService.DownloadAndConvert(textData);
            // You could handle the URL or plain text here
            return bitmap;
        }

        return null;
    }

    public static bool TryGetUrl(IDataObject dataObject, out string url)
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
            Trace.WriteLine(e);
            throw;
        }
    }

    public string GetLocalFileUrl(IDataObject dataObject)
    {
        var path = string.Empty;
        if (dataObject.GetDataPresent(DataFormats.StringFormat))
        {
            path = dataObject.GetData(DataFormats.StringFormat) as string;

            path = path.Trim('"');
        }

        return path;
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