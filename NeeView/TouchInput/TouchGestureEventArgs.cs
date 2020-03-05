using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class TouchGestureEventArgs : EventArgs
    {
        public TouchGesture Gesture { get; set; }
        public bool Handled { get; set; }

        public TouchGestureEventArgs()
        {
        }

        public TouchGestureEventArgs(TouchGesture gesture)
        {
            this.Gesture = gesture;
        }
    }
}
