using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

namespace NeeView.Runtime
{
    public class WindowChromeBehavior
    {
        private Window _window;
        private WindowChrome _windowChrome;
        private bool _isEnabled;


        public WindowChromeBehavior(Window window)
        {
            _window = window;

            _windowChrome = new WindowChrome();
            _windowChrome.CornerRadius = new CornerRadius();
            _windowChrome.UseAeroCaptionButtons = false;
            _windowChrome.CaptionHeight = 0;
            _windowChrome.GlassFrameThickness = new Thickness(1);

            // TODO: Config.Current.Window.WindowChromeFrame によるGlassFrameThicknessの変化
#if false
                if (isGlassFrameEnabled && Config.Current.Window.WindowChromeFrame != WindowChromeFrame.None)
                {
                    _chrome.GlassFrameThickness = new Thickness(1);
                }
                else
                {
                    _chrome.GlassFrameThickness = new Thickness(0);
                }
#endif
        }

        public WindowChrome WindowChrome => _windowChrome;


        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    WindowChrome.SetWindowChrome(_window, _isEnabled ? _windowChrome : null);
                    AddWindowChromeExceptionGuard();
                }
            }
        }


        #region Hotfix: Overflow exception in WindowChrome

        // https://developercommunity.visualstudio.com/content/problem/167357/overflow-exception-in-windowchrome.html?childToView=1209945#comment-1209945 

        private void AddWindowChromeExceptionGuard()
        {
            HwndSource hwnd = (HwndSource)PresentationSource.FromVisual(_window);
            if (hwnd == null) return;
            Debug.WriteLine($"SetHook {hwnd.Handle}");
            hwnd.RemoveHook(HookProc);
            hwnd.AddHook(HookProc);
        }

        private IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0084 /*WM_NCHITTEST*/ )
            {
                // This prevents a crash in WindowChromeWorker._HandleNCHitTest
                try
                {
                    var x = lParam.ToInt32();
                    ////DebugInfo.Current?.SetMessage($"WM_NCHITTEST.LPARAM: {x:#,0}");
                    ////Debug.WriteLine($"{x:#,0}");
                }
                catch (OverflowException)
                {
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        #endregion
    }
}
