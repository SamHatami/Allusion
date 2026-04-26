using System.IO;

namespace Allusion.WPFCore.Service;

public static class SupportedImageFormats
{
    private static readonly HashSet<string> Extensions =
    [
        ".bmp",
        ".dib",
        ".gif",
        ".ico",
        ".jfif",
        ".jpe",
        ".jpeg",
        ".jpg",
        ".png",
        ".tif",
        ".tiff"
    ];

    public static bool IsSupportedFile(string filePath)
    {
        return Extensions.Contains(Path.GetExtension(filePath));
    }
}
