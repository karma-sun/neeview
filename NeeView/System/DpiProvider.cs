using NeeView.Windows;
using System;
using System.Windows;

namespace NeeView
{
    public class DpiProvider : IHasDpiScale
    {
        public event EventHandler DpiChanged;


        /// <summary>
        /// DPI(アプリ値)
        /// </summary>
        public DpiScale Dpi => Config.Current.System.IsIgnoreImageDpi ? RawDpi : OneDpi;

        /// <summary>
        /// DPI(システム値)
        /// </summary>
        public DpiScale RawDpi { get; private set; } = new DpiScale(1, 1);

        /// <summary>
        /// 等倍DPI値
        /// </summary>
        public DpiScale OneDpi { get; private set; } = new DpiScale(1, 1);

        /// <summary>
        /// DPIのXY比率が等しい？
        /// </summary>
        public bool IsDpiSquare => Dpi.DpiScaleX == Dpi.DpiScaleY;


        public bool SetDip(DpiScale dpi)
        {
            if (RawDpi.DpiScaleX != dpi.DpiScaleX || RawDpi.DpiScaleY != dpi.DpiScaleY)
            {
                RawDpi = dpi;
                DpiChanged?.Invoke(null, null);
                return true;
            }
            else
            {
                return false;
            }
        }

        // うーん？
        public DpiScale GetDpiScale()
        {
            return Dpi;
        }
    }
}
