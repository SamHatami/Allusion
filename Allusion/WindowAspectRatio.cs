using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Allusion;

//Entire class taken from
// https://www.mikeobrien.net/blog/maintaining-aspect-ratio-when-resizing
internal class WindowAspectRatio
{
    private double _ratio;

    private WindowAspectRatio(Window window)
    {
        //Sam's addition: Take into account the upper window border 
        _ratio = window.Width / (window.Height-(int)SystemParameters.WindowCaptionHeight);
        ((HwndSource)PresentationSource.FromVisual(window)).AddHook(DragHook);
    }

    public static void Register(Window window)
    {
        new WindowAspectRatio(window);
    }

    internal enum WM
    {
        WINDOWPOSCHANGING = 0x0046
    }

    [Flags()]
    public enum SWP
    {
        NoMove = 0x2
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPOS
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public int flags;
    }

    private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled)
    {
        if ((WM)msg == WM.WINDOWPOSCHANGING)
        {
            var position = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

            if ((position.flags & (int)SWP.NoMove) != 0 ||
                HwndSource.FromHwnd(hwnd).RootVisual == null) return IntPtr.Zero;

            position.cx = (int)(position.cy * _ratio)-(int)SystemParameters.WindowCaptionHeight;

            Marshal.StructureToPtr(position, lParam, true);
            handeled = true;
        }

        return IntPtr.Zero;
    }
}