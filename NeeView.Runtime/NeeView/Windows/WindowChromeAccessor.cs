using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace NeeView.Windows
{
    public class WindowChromeAccessor : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private Window _window;
        private WindowChrome _windowChrome;
        private bool _isEnabled;
        private bool _isSuspended;


        public WindowChromeAccessor(Window window)
        {
            _window = window;

            _windowChrome = new WindowChrome();
            _windowChrome.CornerRadius = new CornerRadius();
            _windowChrome.UseAeroCaptionButtons = false;
            _windowChrome.CaptionHeight = 0;
            _windowChrome.GlassFrameThickness = new Thickness(1);
            _windowChrome.ResizeBorderThickness = new Thickness(4);

            _window.StateChanged += Window_StateChanged;
        }


        public Window Window => _window;

        public WindowChrome WindowChrome => _windowChrome;


        /// <summary>
        /// WindowChromeの有効化
        /// </summary>
        /// <remarks>
        /// WindowHandleが取得できるタイミングで行うこと。
        /// </remarks>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    Update();
                }
            }
        }

        public bool IsSuspended
        {
            get { return _isSuspended; }
            set
            {
                if (SetProperty(ref _isSuspended, value))
                {
                    Update();
                }
            }
        }

        public bool IsActive => _isEnabled && !_isSuspended;



        public double CaptionHeight
        {
            get { return _window.WindowState == WindowState.Maximized ? 24.0 : 28.0; }
        }


        private void Window_StateChanged(object sender, EventArgs e)
        {
            // NOTE: ウィンドウ最大化ではリサイズボーダーを無効にする
            var thickness = (_window.WindowState == WindowState.Maximized) ? 0.0 : 4.0;
            _windowChrome.ResizeBorderThickness = new Thickness(thickness);

            RaisePropertyChanged(nameof(CaptionHeight));
        }

        private void Update()
        {
            WindowChrome.SetWindowChrome(_window, IsActive ? _windowChrome : null);
            AttachWindowChromeExceptionGuard();
        }


        #region Hotfix: Overflow exception in WindowChrome

        // https://developercommunity.visualstudio.com/content/problem/167357/overflow-exception-in-windowchrome.html?childToView=1209945#comment-1209945 

        private HwndSource GetHwndSource()
        {
            return (HwndSource)PresentationSource.FromVisual(_window);
        }

        private void AttachWindowChromeExceptionGuard()
        {
            HwndSource hwnd = GetHwndSource();
            if (hwnd == null) throw new InvalidOperationException("Cannot get window handle.");

            ////Debug.WriteLine($"SetHook {hwnd.Handle}");
            hwnd.RemoveHook(HookProc);
            hwnd.AddHook(HookProc);
        }

        private IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            ////Debug.WriteLine($"{hwnd.ToInt32():X8}: {msg:X4}");

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
