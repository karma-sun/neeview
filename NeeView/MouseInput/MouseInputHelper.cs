using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// MouseInput Helper
    /// </summary>
    public static class MouseInputHelper
    {
        /// <summary>
        /// Deltaを回数に変換
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal static int DeltaCount(MouseWheelEventArgs e)
        {
            int count = Math.Abs(e.Delta) / 120;
            if (count < 1) count = 1;
            return count;
        }

        #region MouseCapture Helper
        // MouseCaptureのラップ。開発用

        //private static volatile int _mouseCaptureSerial = 0;

        [Conditional("DEBUG")]
        public static void DumpMouseCaptureState()
        {
            Debug.WriteLine($"> Mouse captured element: {FixedElementName(Mouse.Captured)}");
        }

        public static void CaptureMouse(object sender, IInputElement element)
        {
            //var id = _mouseCaptureSerial++;
            //Debug.WriteLine($"> {id}.MouseCapture: {FixedElementName(element)} by {sender}");
            element.CaptureMouse();
            //Debug.WriteLine($"> {id}.MouseCapture: done.");
        }

        public static void ReleaseMouseCapture(object sender, IInputElement element)
        {
            //var id = _mouseCaptureSerial++;
            //Debug.WriteLine($"> {id}.ReleaseMouseCapture: {FixedElementName(element)} by {sender}");
            Debug.Assert(Mouse.Captured == element, "WARNING!! It's not caputured element.");
            element.ReleaseMouseCapture();
            //Debug.WriteLine($"> {id}.ReleaseMouseCapture: done.");
        }

        private static string FixedElementName(IInputElement element)
        {
            if (element is FrameworkElement framweorkElement)
            {
                return framweorkElement.ToString() + (framweorkElement.Name != null ? $" ({framweorkElement.Name})" : "");
            }
            else
            {
                return element?.ToString();
            }
        }

        #endregion MouseCapture Helper
    }
}
