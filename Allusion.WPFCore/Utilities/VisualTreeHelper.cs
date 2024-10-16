using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Utilities;

//Not my code, cant recall source
public static class VisualTreeHelpers
{
    public static List<ContentPresenter> FindContentPresentersForImageViews(DependencyObject parent)
    {
        var contentPresenters = new List<ContentPresenter>();

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is ContentPresenter contentPresenter)
            {
                // Check if the ContentPresenter contains an ImageView
                if (contentPresenter.Content is IImageViewModel)
                {
                    contentPresenters.Add(contentPresenter);
                }
            }
            else
            {
                // Recursively search children
                contentPresenters.AddRange(FindContentPresentersForImageViews(child));
            }
        }

        return contentPresenters;
    }

    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
    {
        if (dependencyObject != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);

                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}