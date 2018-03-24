using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class TouchGestureEventArgs : EventArgs
    {
        public RoutedEventArgs RoutedEventArgs { get; set; }
        public TouchGesture Gesture { get; set; }

        public TouchGestureEventArgs()
        {
        }

        public TouchGestureEventArgs(RoutedEventArgs e, TouchGesture gesture)
        {
            this.RoutedEventArgs = e;
            this.Gesture = gesture;
        }
    }
}
