// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using System.Windows;

namespace NeeView
{
    public enum TouchGesture
    {
        None,
        TouchL1,
        TouchL2,
        TouchR1,
        TouchR2,
        TouchCenter,
    }


    public static class TouchGestureExtensions
    {
        //
        public static TouchGesture GetTouchGesture(double xRate, double yRate)
        {
            return TouchGesture.TouchCenter.IsTouched(xRate, yRate)
                ? TouchGesture.TouchCenter
                : GetTouchGestureLast(xRate, yRate);
        }

        //
        public static TouchGesture GetTouchGestureLast(double xRate, double yRate)
        {
            return xRate < 0.5
                ? yRate < 0.5 ? TouchGesture.TouchL1 : TouchGesture.TouchL2
                : yRate < 0.5 ? TouchGesture.TouchR1 : TouchGesture.TouchR2;
        }

        //
        public static bool IsTouched(this TouchGesture self, double xRate, double yRate)
        {
            switch (self)
            {
                case TouchGesture.TouchCenter:
                    return 0.33 < xRate && xRate < 0.66 && yRate < 0.75;
                case TouchGesture.TouchL1:
                    return xRate < 0.5 && yRate < 0.5;
                case TouchGesture.TouchL2:
                    return xRate < 0.5 && !(yRate < 0.5);
                case TouchGesture.TouchR1:
                    return !(xRate < 0.5) && yRate < 0.5;
                case TouchGesture.TouchR2:
                    return !(xRate < 0.5) && !(yRate < 0.5);
                default:
                    return false;
            }
        }
    }

}
