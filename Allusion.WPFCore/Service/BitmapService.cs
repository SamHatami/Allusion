using Allusion.WPFCore.Extensions;
using Allusion.WPFCore.Interfaces;
using Allusion.WPFCore.Utilities;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Service;

public class BitmapService : IBitmapService
{
    private readonly HttpClient _httpClient;

    public BitmapService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public BitmapImage? GetFromUri(string? uriString)
    {
        if (!File.Exists(uriString)) return null;

        return Application.Current.Dispatcher.CheckAccess()
            ? BitmapUtils.LoadImageFromUri(uriString)
            : Application.Current.Dispatcher.Invoke(() => BitmapUtils.LoadImageFromUri(uriString));
    }

    public BitmapImage[]? LoadFromUri(string[]? fileUriStrings)
    {
        if (fileUriStrings == null) return null;

        List<BitmapImage> bitmaps = [];
        var existingFiles = fileUriStrings.Where(File.Exists);

        foreach (var file in existingFiles)
            try
            {
                bitmaps.Add(new BitmapImage(new Uri(file)));
            }
            catch (Exception e)
            {
                StaticLogger.Error("Could not get from URI", true, e.Message);
            }

        return bitmaps.ToArray();
    }

    public BitmapImage LoadFromUri(string uri)
    {
        return new BitmapImage(new Uri(uri));
    }

    public async Task<BitmapImage?> DownloadAndConvert(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url)) return null;

        try
        {
            byte[] imageBytes;
            if (url.StartsWith("data:image"))
            {
                var base64Data = url.Substring(url.IndexOf(",") + 1);
                imageBytes = Convert.FromBase64String(base64Data);
            }
            else
            {
                imageBytes = await _httpClient.GetByteArrayAsync(url, cancellationToken);
            }

            var bitmapSource = BitmapUtils.CreateFromBytes(imageBytes);
            if (bitmapSource == null)
            {
                StaticLogger.Error("Failed to create bitmap from downloaded data", false, string.Empty);
                return null;
            }

            return bitmapSource.ConvertToBitmapImage();
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled, return null
            return null;
        }
        catch (HttpRequestException e)
        {
            StaticLogger.Error($"HTTP error downloading image from {url}", true, e.Message);
            return null;
        }
        catch (Exception e)
        {
            StaticLogger.Error($"Failed to download and convert image from {url}", true, e.Message);
            return null;
        }
    }
}