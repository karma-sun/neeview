// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using System.Windows;

namespace NeeView
{
    public enum TouchGesture
    {
        None,
        TouchLeft,
        TouchRight,
        TouchCenter,
    }


    public static class TouchGestureExtensions
    {
        //
        public static TouchGesture GetTouchGesture(double xRate, double yRate)
        {
            return TouchGesture.TouchCenter.IsTouched(xRate, yRate)
                ? TouchGesture.TouchCenter
                : xRate < 0.5 ? TouchGesture.TouchLeft : TouchGesture.TouchRight;
        }

        //
        public static bool IsTouched(this TouchGesture self, double xRate, double yRate)
        {
            switch (self)
            {
                case TouchGesture.TouchCenter:
                    return 0.33 < xRate && xRate < 0.66 && yRate < 0.75;
                case TouchGesture.TouchLeft:
                    return xRate < 0.5;
                case TouchGesture.TouchRight:
                    return !(xRate < 0.5);
                default:
                    return false;
            }
        }
    }

}
