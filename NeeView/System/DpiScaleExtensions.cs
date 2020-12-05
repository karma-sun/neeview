using NeeView.Windows;
using System;
using System.Windows;

namespace NeeView
{
    public static class DpiScaleExtensions
    {
        public static DpiScale OneDpiScale { get; } = new DpiScale(1, 1);

        /// <summary>
        /// IsIgnoreImageDpiを適用したDpiScalenに変換
        /// </summary>
        public static DpiScale ToFixedScale(this DpiScale self)
        {
            return Config.Current.System.IsIgnoreImageDpi? self : OneDpiScale;
        }
    }
}
