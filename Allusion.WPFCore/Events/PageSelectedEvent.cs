using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Events;

public class PageSelectedEvent(IPageViewModel page)
{
    public IPageViewModel Page { get; } = page;
}