using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Utilities;

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
}