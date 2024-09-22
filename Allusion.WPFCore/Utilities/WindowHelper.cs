using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Allusion.WPFCore.Utilities
{
    public static class WindowHelper
    {

        public static Size CurrentScreenSize(Window window)
        {
            var winSize = window.RenderSize;

            return winSize;
        }
    }
}
