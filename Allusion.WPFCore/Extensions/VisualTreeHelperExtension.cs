using System.Windows;
using System.Windows.Media;

namespace Allusion.WPFCore.Extensions;

public static class VisualTreeHelperExtensions
{
    public static T FindVisualAncestor<T>(this DependencyObject element, Func<T, bool> predicate = null) where T : DependencyObject
    {
        while (element != null && !(element is T t && (predicate == null || predicate(t))))
        {
            element = VisualTreeHelper.GetParent(element);
        }
        return element as T;
    }
}