using System.Diagnostics;
using System.Windows;

namespace Allusion.Core.Extensions;

public static class DataObject
{
    public static string ExtractUrlFromDataObject(this IDataObject? dataObject)
    {
        var Url = string.Empty;

        if (dataObject is null) return Url;

        foreach (var format in dataObject.GetFormats())
        {
            var data = dataObject.GetData(DataFormats.Html);
            // Inspect data for potential URL or source information
            Debug.WriteLine($"Format: {format}, Data: {data}");
        }

        return Url;
    }
}