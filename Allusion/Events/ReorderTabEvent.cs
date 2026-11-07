using Allusion.ViewModels;

namespace Allusion.Events
{
    public class ReorderTabEvent
    {
        public PageViewModel Source { get; }
        public int TargetIndex { get; }
        public ReorderTabEvent(PageViewModel source, int targetIndex)
        {
            Source = source;
            TargetIndex = targetIndex;
        }
    }
}
