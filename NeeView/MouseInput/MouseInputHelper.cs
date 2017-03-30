// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
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
    }

}
