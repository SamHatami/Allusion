﻿using Allusion.WPFCore.Extensions;
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

    public async Task<BitmapImage?> DownloadAndConvert(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        using var client = new HttpClient();

        BitmapSource bitmapSource = null;
        byte[] imageBytes = null;
        try
        {
            if (url.StartsWith("data:image"))
            {
                var base64Data = url.Substring(url.IndexOf(",") + 1);
                imageBytes = Convert.FromBase64String(base64Data);
            }
            else
            {
                imageBytes = await client.GetByteArrayAsync(url);

            }

            bitmapSource = BitmapUtils.CreateFromBytes(imageBytes);
        }
        catch (Exception e)
        {
            StaticLogger.Error("Could fetch image", true, e.Message);
        }

        // Create a MemoryStream from the byte array

        Debug.Assert(bitmapSource != null, nameof(bitmapSource) + " != null");

        return bitmapSource.ConvertToBitmapImage();
    }
}