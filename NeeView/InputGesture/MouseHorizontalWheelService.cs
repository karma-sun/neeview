using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NeeView
{
    public class MouseHorizontalWheelService : INotifyMouseHorizontalWheelChanged
    {
        internal static class NativeMethods
        {
            internal const int WM_MOUSEHWHEEL = 0x020E;

            internal static ushort HIWORD(IntPtr dwValue)
            {
                return (ushort)((((long)dwValue) >> 0x10) & 0xffff);
            }

            internal static ushort HIWORD(uint dwValue)
            {
                return (ushort)(dwValue >> 0x10);
            }

            internal static int GET_WHEEL_DELTA_WPARAM(IntPtr wParam)
            {
                return (short)HIWORD(wParam);
            }

            internal static int GET_WHEEL_DELTA_WPARAM(uint wParam)
            {
                return (short)HIWORD(wParam);
            }
        }


        private Window _window;


        public MouseHorizontalWheelService(Window window)
        {
            _window = window;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
            {
                HwndSource.FromHwnd(hwnd).AddHook(WndProc);
            }
            else
            {
                _window.Loaded += Window_Loaded;
            }
        }


        public event MouseWheelEventHandler MouseHorizontalWheelChanged;


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _window.Loaded -= Window_Loaded;

            var hwnd = new WindowInteropHelper(_window).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeMethods.WM_MOUSEHWHEEL:
                    MouseWheelEventArgs args = null;
                    try
                    {
                        // NOTE: デバイスが特定できないのでとりあえず Mouse.PrimaryDevice を使用
                        // NOTE: ReferenceSource を見る限り Mouse.PrimaryDevice が null になることはないようだが、念の為に確認している
                        if (Mouse.PrimaryDevice != null)
                        {
                            var delta = NativeMethods.GET_WHEEL_DELTA_WPARAM(wParam);
                            args = new MouseWheelEventArgs(Mouse.PrimaryDevice, System.Environment.TickCount, delta);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    if (args != null)
                    {
                        MouseHorizontalWheelChanged?.Invoke(_window, args);
                        handled = args.Handled;
                    }
                    break;
            }

            return IntPtr.Zero;
        }
    }

}
