// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // 開始時の基準座標
    public enum DragViewOrigin
    {
        Center, // コンテンツの中心
        LeftTop, // コンテンツの左上
        RightTop, // コンテンツの右上
    }

    public static class ViewOriginExtensions
    {
        public static DragViewOrigin Reverse(this DragViewOrigin origin)
        {
            switch (origin)
            {
                case DragViewOrigin.LeftTop: return DragViewOrigin.RightTop;
                case DragViewOrigin.RightTop: return DragViewOrigin.LeftTop;
                default: return origin;
            }
        }
    }
}
