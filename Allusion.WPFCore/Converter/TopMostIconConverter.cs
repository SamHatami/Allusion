using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Allusion.WPFCore.Converter;

public class TopMostIconConverter : IValueConverter
{
    private static readonly SolidColorBrush OnBrush = new(Colors.Green);
    private static readonly SolidColorBrush OffBrush = new(Colors.Black);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? OnBrush : OffBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
