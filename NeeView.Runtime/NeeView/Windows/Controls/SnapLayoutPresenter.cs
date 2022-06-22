using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// Windows11 の SnapLayout サポート
    /// </summary>
    // from https://bitbucket.org/neelabo/neeview/issues/1183/windows-11
    // from https://stackoverflow.com/questions/69797178/support-windows-11-snap-layout-in-wpf-app
    public class SnapLayoutPresenter
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_NCMOUSELEAVE = 0x02A2;

        private const int HTMINBUTTON = 8;
        private const int HTMAXBUTTON = 9;

        private readonly SolidColorBrush _defaultButtonBackground = Brushes.Transparent;
        private readonly SolidColorBrush _mouseOverButtonBackground = new SolidColorBrush(Color.FromArgb(0x66, 0x88, 0x88, 0x88));
        public IHasMaximizeButton _maximizeButton;


        public SnapLayoutPresenter(IHasMaximizeButton maximizeButton)
        {
            _maximizeButton = maximizeButton;
        }


        /// <summary>
        /// Attach SnapLayout
        /// </summary>
        /// <param name="window"></param>
        public void Attach(Window window)
        {
            if (window is null) return;
            if (!Windows11Tools.IsWindows11OrGreater) return;

            var hwnd = PresentationSource.FromVisual(window) as System.Windows.Interop.HwndSource;
            hwnd?.AddHook(WndProc);
        }

        /// <summary>
        /// Detach SnapLayout
        /// </summary>
        /// <param name="window"></param>
        public void Detach(Window window)
        {
            if (window is null) return;
            if (!Windows11Tools.IsWindows11OrGreater) return;

            var hwnd = PresentationSource.FromVisual(window) as System.Windows.Interop.HwndSource;
            hwnd?.RemoveHook(WndProc);
        }

        /// <summary>
        /// WinProc for SnapLayout
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_NCHITTEST:
                    return OnNCHitTest(lParam, ref handled);

                case WM_NCLBUTTONDOWN:
                    return OnNCLButtonDown(lParam, ref handled);

                case WM_NCMOUSELEAVE:
                    return OnNCMouseLeave(lParam, ref handled);

                default:
                    return IntPtr.Zero;
            }
        }

        /// <summary>
        /// WM_NCHITTEST
        /// </summary>
        private IntPtr OnNCHitTest(IntPtr lParam, ref bool handled)
        {
            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            if (HitTest(button, lParam))
            {
                _maximizeButton.SetMaximizeButtonBackground(_mouseOverButtonBackground);
                handled = true;
                return new IntPtr(HTMAXBUTTON);
            }
            else
            {
                _maximizeButton.SetMaximizeButtonBackground(_defaultButtonBackground);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// WM_NCLBUTTONDOWN
        /// </summary>
        private IntPtr OnNCLButtonDown(IntPtr lParam, ref bool handled)
        {
            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            if (HitTest(button, lParam))
            {
                handled = true;
                IInvokeProvider invokeProv = new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv?.Invoke();
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// WM_NCMOUSELEAVE
        /// </summary>
        private IntPtr OnNCMouseLeave(IntPtr lParam, ref bool handled)
        {
            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            _maximizeButton.SetMaximizeButtonBackground(_defaultButtonBackground);
            return IntPtr.Zero;
        }

        /// <summary>
        /// ウィンドウメッセージ座標でのコントロール当たり判定
        /// </summary>
        /// <param name="element">コントロール</param>
        /// <param name="lParam">ウィンドウメッセージのLPARAM(座標)</param>
        /// <returns></returns>
        private static bool HitTest(FrameworkElement element, IntPtr lParam)
        {
            if (element is null || !element.IsVisible)
            {
                return false;
            }

            var dpi = VisualTreeHelper.GetDpi(element);
            var rect = new Rect(element.PointToScreen(new Point()), new Size(element.ActualWidth * dpi.DpiScaleX, element.ActualHeight * dpi.DpiScaleY));
            short x = GET_X_LPARAM(lParam);
            short y = GET_Y_LPARAM(lParam);
            return rect.Contains(x, y);

            short GET_X_LPARAM(IntPtr lp)
            {
                return (short)(ushort)((uint)lp.ToInt32() & 0xffff);
            }

            short GET_Y_LPARAM(IntPtr lp)
            {
                return (short)(ushort)((uint)lp.ToInt32() >> 16);
            }
        }
    }
}
