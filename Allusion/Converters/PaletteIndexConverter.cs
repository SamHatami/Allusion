using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Allusion.Converters
{
    public class PaletteIndexConverter : IValueConverter
    {
        private static readonly Brush[] _palette = new Brush[]
        {
            Brushes.Brown, Brushes.DarkSlateGray, Brushes.Gray, Brushes.DarkGray, Brushes.DarkGoldenrod,
            Brushes.DarkOliveGreen, Brushes.DarkGreen, Brushes.DarkSlateBlue, Brushes.DarkMagenta,
            Brushes.DarkCyan, Brushes.DarkRed, Brushes.DarkBlue, Brushes.DarkKhaki, Brushes.DarkOrange,
            Brushes.DarkViolet, Brushes.DarkTurquoise, Brushes.DarkSalmon, Brushes.Peru, Brushes.DarkOrchid,
            Brushes.DarkGreen, Brushes.DarkGoldenrod, Brushes.DarkOliveGreen, Brushes.DarkOrange,
            Brushes.DarkSlateBlue, Brushes.DarkCyan, Brushes.DarkMagenta, Brushes.DarkRed,
            Brushes.DarkBlue, Brushes.DarkGreen, Brushes.DarkGoldenrod, Brushes.DarkOliveGreen
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int idx = (value is int i) ? i : 0;
            idx = Math.Clamp(idx, 0, _palette.Length - 1);
            return _palette[idx];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
