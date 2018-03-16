// from http://grabacr.net/archives/1585
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{
    #region Native

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public SW showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    public enum SW
    {
        HIDE = 0,
        SHOWNORMAL = 1,
        SHOWMINIMIZED = 2,
        SHOWMAXIMIZED = 3,
        SHOWNOACTIVATE = 4,
        SHOW = 5,
        MINIMIZE = 6,
        SHOWMINNOACTIVE = 7,
        SHOWNA = 8,
        RESTORE = 9,
        SHOWDEFAULT = 10,
    }

    #endregion

    /// <summary>
    /// Window Placement
    /// </summary>
    public class WindowPlacement
    {
        public static WindowPlacement Current { get; private set; }

        #region NativeApi

        internal static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

            [DllImport("user32.dll")]
            public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
        }

        #endregion

        #region Fields

        private Window _window;

        #endregion

        #region Constructors

        //
        public WindowPlacement(Window window)
        {
            Current = this;

            _window = window;
            _window.SourceInitialized += Window_SourceInitialized;
            _window.Closing += Window_Closing;
        }

        #endregion

        #region Properties

        public WINDOWPLACEMENT? Placement { get; set; }

        public double Width { get; set; } = 640.0;
        public double Height { get; set; } = 480.0;

        #endregion

        #region Methods

        //
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (this.Placement.HasValue)
            {
                var hwnd = new WindowInteropHelper(_window).Handle;
                var placement = this.Placement.Value;
                placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                placement.flags = 0;
                placement.showCmd = (placement.showCmd == SW.SHOWMINIMIZED) ? SW.SHOWNORMAL : placement.showCmd;

                placement.normalPosition.Right = placement.normalPosition.Left + (int)(this.Width * Config.Current.Dpi.DpiScaleX + 0.5);
                placement.normalPosition.Bottom = placement.normalPosition.Top + (int)(this.Height * Config.Current.Dpi.DpiScaleY + 0.5);
                //Debug.WriteLine($">>>> Restore.WIDTH: {placement.normalPosition.Right - placement.normalPosition.Left}, DPI: {Config.Current.Dpi.DpiScaleX}");

                NativeMethods.SetWindowPlacement(hwnd, ref placement);
            }
        }

        //
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StorePlacement();
        }

        //
        public void StorePlacement()
        {
            var hwnd = new WindowInteropHelper(_window).Handle;
            if (hwnd == IntPtr.Zero) return;

            NativeMethods.GetWindowPlacement(hwnd, out WINDOWPLACEMENT placement);

            this.Width = (placement.normalPosition.Right - placement.normalPosition.Left) / Config.Current.Dpi.DpiScaleX;
            this.Height = (placement.normalPosition.Bottom - placement.normalPosition.Top) / Config.Current.Dpi.DpiScaleY;
            //Debug.WriteLine($">>>> Store.WIDTH: {placement.normalPosition.Right - placement.normalPosition.Left}, DPI: {Config.Current.Dpi.DpiScaleX}");

            this.Placement = placement;
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public WINDOWPLACEMENT? Placement { get; set; }
            [DataMember]
            public double Width { get; set; }
            [DataMember]
            public double Height { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            StorePlacement();

            var memento = new Memento();
            memento.Placement = this.Placement;
            memento.Width = this.Width;
            memento.Height = this.Height;
            
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.Placement = memento.Placement;
            this.Width = memento.Width;
            this.Height = memento.Height;
        }

        #endregion
    }
}
