using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Allusion.WPFCore.Interfaces;
using HtmlAgilityPack;
using IDataObject = System.Windows.IDataObject;

namespace Allusion.WPFCore.Extensions;

public class DataObjectImageExtractor
{
    private readonly IBitmapService _bitmapService;

    public DataObjectImageExtractor(IBitmapService bitmapService)
    {
        _bitmapService = bitmapService;
    }

    public BitmapSource[]? GetBitmapFromLocal(IDataObject dataObject)
    {
        var formats = dataObject.GetFormats(true);
        if (formats == null || formats.Length == 0) return null;

        if (formats.Contains("FileContents"))
        {
            var filePaths = dataObject.GetData("FileContents") as string[];
            return _bitmapService.LoadFromUri(filePaths).OfType<BitmapSource>().ToArray();
        }

        if (formats.Contains("FileName"))
        {
            var filePaths = dataObject.GetData("FileName") as string[];
            return _bitmapService.LoadFromUri(filePaths).OfType<BitmapSource>().ToArray();
        }

        if (formats.Contains("PNG"))
        {
            Debug.WriteLine("PNG");

            using var ms = (MemoryStream)dataObject.GetData("PNG");
            ms.Position = 0;
        }

        return null;
    }

    public async Task<BitmapImage?> GetWebBitmapAsync(IDataObject dataObject, CancellationToken cancellationToken = default)
    {
        if (dataObject.GetDataPresent(DataFormats.Bitmap))
            return dataObject.GetData(DataFormats.Bitmap) as BitmapImage;

        if (dataObject.GetDataPresent(DataFormats.Html))
        {
            if (TryGetUrl(dataObject, out var imageUrl))
                return await _bitmapService.DownloadAndConvert(imageUrl, cancellationToken);
        }
        else if (dataObject.GetDataPresent(DataFormats.Text))
        {
            var textData = dataObject.GetData(DataFormats.Text) as string;
            return IsDownloadableImageReference(textData)
                ? await _bitmapService.DownloadAndConvert(textData, cancellationToken)
                : null;
        }

        return null;
    }

    public string GetLocalFileUrl(IDataObject dataObject)
    {
        var path = string.Empty;
        if (dataObject.GetDataPresent(DataFormats.StringFormat))
        {
            path = dataObject.GetData(DataFormats.StringFormat) as string ?? string.Empty;
            path = path.Trim('"');
        }

        return path;
    }

    private bool TryGetUrl(IDataObject dataObject, out string url)
    {
        if (!dataObject.GetDataPresent(DataFormats.Html))
        {
            url = string.Empty;
            return false;
        }

        var htmlData = dataObject.GetData(DataFormats.Html) as string;
        url = GetImageUrlFromHtml(htmlData);
        return !string.IsNullOrEmpty(url);
    }

    private static string GetImageUrlFromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var node = htmlDoc.DocumentNode.SelectSingleNode("//img");
        if (node is null) return string.Empty;

        var imageSource = GetImageAttribute(node);
        if (string.IsNullOrWhiteSpace(imageSource)) return string.Empty;

        imageSource = NormalizeImageUrl(imageSource, GetHtmlSourceUrl(html));
        return IsDownloadableImageReference(imageSource) ? imageSource : string.Empty;
    }

    private static string GetImageAttribute(HtmlNode node)
    {
        foreach (var attributeName in new[] { "src", "data-src", "data-original", "data-lazy-src" })
        {
            var value = node.GetAttributeValue(attributeName, string.Empty);
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }

        var srcSet = node.GetAttributeValue("srcset", string.Empty);
        return srcSet
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(candidate => candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty)
            .FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate)) ?? string.Empty;
    }

    private static string NormalizeImageUrl(string imageUrl, string sourceUrl)
    {
        if (imageUrl.StartsWith("//", StringComparison.Ordinal))
            return $"https:{imageUrl}";

        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var absoluteUri))
            return absoluteUri.ToString();

        if (Uri.TryCreate(sourceUrl, UriKind.Absolute, out var baseUri) &&
            Uri.TryCreate(baseUri, imageUrl, out var resolvedUri))
        {
            return resolvedUri.ToString();
        }

        return imageUrl;
    }

    private static string GetHtmlSourceUrl(string html)
    {
        using var reader = new StringReader(html);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            const string sourceUrlPrefix = "SourceURL:";
            if (line.StartsWith(sourceUrlPrefix, StringComparison.OrdinalIgnoreCase))
                return line[sourceUrlPrefix.Length..].Trim();

            if (line.StartsWith("<", StringComparison.Ordinal))
                break;
        }

        return string.Empty;
    }

    private static bool IsDownloadableImageReference(string? imageReference)
    {
        if (string.IsNullOrWhiteSpace(imageReference)) return false;
        if (imageReference.StartsWith("data:image", StringComparison.OrdinalIgnoreCase)) return true;

        return Uri.TryCreate(imageReference, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
