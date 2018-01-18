// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
