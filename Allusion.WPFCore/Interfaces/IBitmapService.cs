using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Allusion.WPFCore.Interfaces
{
    public interface IBitmapService
    {
        public BitmapImage? GetFromUri(string? uriString);
        public BitmapImage?[] LoadFromUri(string[]? fileUriStrings);
        public BitmapImage? LoadFromUri(string uri);
        public Task<BitmapImage?>? DownloadAndConvert(string url);

    }
}
