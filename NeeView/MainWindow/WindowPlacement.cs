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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{
    #region Native

    [Serializable]
    [DataContract]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        [DataMember] public int length;
        [DataMember] public int flags;
        [DataMember] public SW showCmd;
        [DataMember] public POINT minPosition;
        [DataMember] public POINT maxPosition;
        [DataMember] public RECT normalPosition;

        public int Length
        {
            get => length;
            set => length = value;
        }

        [JsonIgnore]
        public int Flags
        {
            get => flags;
            set => flags = value;
        }

        public SW ShowCmd
        {
            get => showCmd;
            set => showCmd = value;
        }

        public POINT MinPosition
        {
            get => minPosition;
            set => minPosition = value;
        }

        public POINT MaxPosition
        {
            get => maxPosition;
            set => maxPosition = value;
        }

        public RECT NormalPosition
        {
            get => normalPosition;
            set => normalPosition = value;
        }

        public bool IsValid() => length == Marshal.SizeOf(typeof(WINDOWPLACEMENT));
    }

    [Serializable]
    [DataContract]
    [JsonConverter(typeof(POINT.JsonNativePointConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        [DataMember] public int X;
        [DataMember] public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return $"{X},{Y}";
        }

        public static POINT Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new InvalidCastException();

            var tokens = s.Split(',');
            if (tokens.Length != 2) throw new InvalidCastException();

            return new POINT(int.Parse(tokens[0]), int.Parse(tokens[1]));
        }

        public sealed class JsonNativePointConverter : JsonConverter<POINT>
        {
            public override POINT Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return POINT.Parse(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, POINT value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }


    [Serializable]
    [DataContract]
    [JsonConverter(typeof(RECT.JsonNativeRectConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        [DataMember] public int Left;
        [DataMember] public int Top;
        [DataMember] public int Right;
        [DataMember] public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public override string ToString()
        {
            return $"{Left},{Top},{Right},{Bottom}";
        }

        public static RECT Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new InvalidCastException();

            var tokens = s.Split(',');
            if (tokens.Length != 4) throw new InvalidCastException();

            return new RECT(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]));
        }

        public sealed class JsonNativeRectConverter : JsonConverter<RECT>
        {
            public override RECT Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return RECT.Parse(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, RECT value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
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
        static WindowPlacement() => Current = new WindowPlacement();
        public static WindowPlacement Current { get; }

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

        private WindowPlacement()
        {
            _window = MainWindow.Current;
            _window.SourceInitialized += Window_SourceInitialized;
            _window.Closing += Window_Closing;
        }

        #endregion

        #region Properties

#if false
        public WINDOWPLACEMENT? Placement { get; set; }

        public double Width { get; set; } = 640.0;
        public double Height { get; set; } = 480.0;
#endif

        public bool IsMaximized { get; set; }

        #endregion

        #region Methods

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            SetPlacement();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StorePlacement();
        }

        private void SetPlacement()
        {
            if (Config.Current.Window.Placement.IsValid())
            {
                var hwnd = new WindowInteropHelper(_window).Handle;
                var placement = Config.Current.Window.Placement;
                placement.Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                placement.Flags = 0;
                placement.ShowCmd = IsMaximized ? SW.SHOWMAXIMIZED : SW.SHOWNORMAL;

                placement.normalPosition.Right = placement.normalPosition.Left + (int)(Config.Current.Window.Width * Environment.Dpi.DpiScaleX + 0.5);
                placement.normalPosition.Bottom = placement.normalPosition.Top + (int)(Config.Current.Window.Height * Environment.Dpi.DpiScaleY + 0.5);
                //Debug.WriteLine($">>>> Restore.WIDTH: {placement.normalPosition.Right - placement.normalPosition.Left}, DPI: {Config.Current.Dpi.DpiScaleX}");

                NativeMethods.SetWindowPlacement(hwnd, ref placement);
            }
        }

        public void StorePlacement()
        {
            var hwnd = new WindowInteropHelper(_window).Handle;
            if (hwnd == IntPtr.Zero) return;

            NativeMethods.GetWindowPlacement(hwnd, out WINDOWPLACEMENT placement);

            Config.Current.Window.Width = (placement.normalPosition.Right - placement.normalPosition.Left) / Environment.Dpi.DpiScaleX;
            Config.Current.Window.Height = (placement.normalPosition.Bottom - placement.normalPosition.Top) / Environment.Dpi.DpiScaleY;
            //Debug.WriteLine($">>>> Store.WIDTH: {placement.normalPosition.Right - placement.normalPosition.Left}, DPI: {Config.Current.Dpi.DpiScaleX}");

            Config.Current.Window.Placement = placement;
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public WINDOWPLACEMENT? Placement { get; set; }

            [DataMember]
            public double Width { get; set; }
            [DataMember]
            public double Height { get; set; }

            public void RestoreConfig()
            {
                Config.Current.Window.Placement = Placement ?? default;
                Config.Current.Window.Width = Width;
                Config.Current.Window.Height = Height;
            }
        }

        public Memento CreateMemento()
        {
            StorePlacement();

            var memento = new Memento();
            memento.Placement = Config.Current.Window.Placement;
            memento.Width = Config.Current.Window.Width;
            memento.Height = Config.Current.Window.Height;

            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            ////this.Placement = memento.Placement;
            ////this.Width = memento.Width;
            ////this.Height = memento.Height;
        }

        #endregion
    }
}
