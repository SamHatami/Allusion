using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Allusion.WPFCore.Converter;

public class ZoomedThicknessConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var zoom = values.Length > 0 ? GetDouble(values[0], 1.0, culture) : 1.0;
        var thickness = values.Length > 1 ? GetDouble(values[1], 1.0, culture) : 1.0;

        if (zoom <= 0)
            zoom = 1.0;
        if (thickness < 0)
            thickness = 0;

        return new Thickness(thickness / zoom);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static double GetDouble(object? value, double fallback, CultureInfo culture)
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
