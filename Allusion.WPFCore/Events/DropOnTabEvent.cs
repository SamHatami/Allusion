using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allusion.WPFCore.Interfaces;

namespace Allusion.WPFCore.Events
{
    public class DropOnTabEvent
    {
        public IImageViewModel[] ImageVM { get; }
        public IPageViewModel TargetPage { get; }

        public DropOnTabEvent(IImageViewModel[] imageVm, IPageViewModel targetPage)
        {
            ImageVM = imageVm;
            TargetPage = targetPage;
        }
    }
}
