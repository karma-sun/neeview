// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows.Input;

namespace NeeView
{
    //
    public class TouchGestureEventArgs : EventArgs
    {
        public TouchEventArgs TouchEventArgs { get; set; }
        public TouchGesture Gesture { get; set; }

        public TouchGestureEventArgs()
        {
        }

        public TouchGestureEventArgs(TouchEventArgs e, TouchGesture gesture)
        {
            this.TouchEventArgs = e;
            this.Gesture = gesture;
        }
    }
}
