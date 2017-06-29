// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    //
    public class TouchContext
    {
        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// ドラッグ開始時間
        /// </summary>
        public int StartTimestamp { get; set; }

        /// <summary>
        /// デバイス
        /// </summary>
        public StylusDevice StylusDevice { get; set; }
    }
}
