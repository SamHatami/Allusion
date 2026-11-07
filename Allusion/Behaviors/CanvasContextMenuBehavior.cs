using System.Windows;
using System.Windows.Controls;
using Allusion.ViewModels;
using Caliburn.Micro;
using Microsoft.Xaml.Behaviors;

namespace Allusion.Behaviors
{
    public class CanvasContextMenuBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            var menu = new ContextMenu();
            var createItem = new MenuItem { Header = "Create new group" };
            createItem.Click += (s, args) =>
            {
                if (AssociatedObject.DataContext is PageViewModel page)
                {
                    var group = new GroupViewModel { Title = "no name", X = 50, Y = 50, Width = 320, Height = 220 };
                    if (page.SelectedImages.Count > 0)
                    {
                        foreach (var img in page.SelectedImages.ToArray())
                            group.Images.Add(img);
                    }
                    page.Groups.Add(group);
                }
            };
            menu.Items.Add(createItem);
            AssociatedObject.ContextMenu = menu;
        }
    }
}
