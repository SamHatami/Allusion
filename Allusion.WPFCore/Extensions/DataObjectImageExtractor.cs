using System.Runtime.CompilerServices;
using Allusion.WPFCore.Service;
using HtmlAgilityPack;
using System.Windows;
using System.Windows.Media.Imaging;
using IDataObject = System.Windows.IDataObject;
using System.Diagnostics;
using System.IO;
using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Extensions;

public class DataObjectImageExtractor
{
    private readonly IBitmapService _bitmapService;

    public DataObjectImageExtractor(IBitmapService bitmapService)
    {
        _bitmapService = bitmapService;
    }

    public BitmapSource[] GetBitmapFromLocal(IDataObject dataObject)
    {
        //This is just so crazy...
        List<BitmapSource> bitmaps = [];

        //Get formats for this dataobject as helper for conversion
        var formats = dataObject.GetFormats(true);
        if (formats == null || formats.Length == 0) return null;

        //Multiple file paste from explorer
        if (formats.Contains("FileContents"))
        {
            var filePaths = dataObject.GetData("FileContents") as string[];

            return _bitmapService.LoadFromUri(filePaths);
        }

        //if the SourceImage was pasted from explorer
        if (formats.Contains("FileName"))
        {
            string[] filePaths = dataObject.GetData("FileName") as string[];

            return _bitmapService.LoadFromUri(filePaths);
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

    public async Task<BitmapImage?> GetWebBitmapAsync(IDataObject dataObject)
    {
        if (dataObject.GetDataPresent(DataFormats.Bitmap))
            return dataObject.GetData(DataFormats.Bitmap) as BitmapImage;
        
        // Check if the data contains HTML (could include the SourceImage URL)
        if (dataObject.GetDataPresent(DataFormats.Html))
        {
            BitmapImage? bitmap = null;
            if (TryGetUrl(dataObject, out var imageUrl)) 
                return await _bitmapService.DownloadAndConvert(imageUrl);

        }
        // Check if the data contains plain text. Need to check if its a valid url ?
        else if (dataObject.GetDataPresent(DataFormats.Text))
        {
            var textData = dataObject.GetData(DataFormats.Text) as string;
            return await _bitmapService.DownloadAndConvert(textData);
            // You could handle the URL or plain text here
        }

        return null;
    }

    private bool TryGetUrl(IDataObject dataObject, out string url)
    {
        if (!dataObject.GetDataPresent(DataFormats.Html))
        {
            url = string.Empty;
            return false;
        }

        var htmlData = dataObject.GetData(DataFormats.Html) as string;
        url = HtmlTest(htmlData);
        return !string.IsNullOrEmpty(url);
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
        if (node is null) return String.Empty;
        var imageSource = node.Attributes["src"].Value;

        return imageSource;
    }

}