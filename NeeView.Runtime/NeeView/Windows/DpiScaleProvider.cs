using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView.Windows
{
    public class DpiScaleProvider : IDpiScaleProvider
    {
        public event EventHandler DpiChanged;


        public DpiScale DpiScale { get; private set; } = new DpiScale(1, 1);


        public bool SetDipScale(DpiScale dpi)
        {
            if (DpiScale.DpiScaleX != dpi.DpiScaleX || DpiScale.DpiScaleY != dpi.DpiScaleY)
            {
                DpiScale = dpi;
                DpiChanged?.Invoke(this, null);
                return true;
            }
            else
            {
                return false;
            }
        }

        public DpiScale GetDpiScale()
        {
            return DpiScale;
        }
    }

}
