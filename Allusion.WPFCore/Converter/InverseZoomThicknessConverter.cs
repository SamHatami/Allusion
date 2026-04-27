using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Allusion.WPFCore.Converter;

public class InverseZoomThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var thickness = GetDouble(parameter, 1.0, culture);
        var zoom = GetDouble(value, 1.0, culture);

        if (zoom <= 0)
            zoom = 1.0;

        return new Thickness(thickness / zoom);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static double GetDouble(object value, double fallback, CultureInfo culture)
    {
        return value switch
        {
            double number => number,
            string text when double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) => number,
            string text when double.TryParse(text, NumberStyles.Float, culture, out var number) => number,
            _ => fallback
        };
    }
}
