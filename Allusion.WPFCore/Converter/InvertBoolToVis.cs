using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Allusion.WPFCore.Converter
{
    //https://stackoverflow.com/questions/534575/how-do-i-invert-booleantovisibilityconverter

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertBoolToVis : IValueConverter
    {
        enum Parameters
        {
            Normal, Inverted
        }

        public object Convert(object? value, Type targetType,
            object parameter, CultureInfo culture)
        {

            return value is null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
