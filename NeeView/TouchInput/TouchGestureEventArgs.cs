using System;
using System.Windows.Input;

namespace NeeView
{
    //
    public class TouchGestureEventArgs : EventArgs
    {
        public StylusEventArgs TouchEventArgs { get; set; }
        public TouchGesture Gesture { get; set; }

        public TouchGestureEventArgs()
        {
        }

        public TouchGestureEventArgs(StylusEventArgs e, TouchGesture gesture)
        {
            this.TouchEventArgs = e;
            this.Gesture = gesture;
        }
    }
}
