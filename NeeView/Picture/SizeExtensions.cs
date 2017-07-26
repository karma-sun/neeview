// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows;

namespace NeeView
{
    public static class SizeExtensions
    {
        // 少数切り捨てサイズを返す
        public static Size Truncate(this Size self)
        {
            return self.IsEmpty ? self : new Size((int)self.Width, (int)self.Height);
        }

        // 画像アスペクト比を保つ最大のサイズを返す
        public static Size Uniformed(this Size self, Size target)
        {
            var rateX = self.Width / target.Width;
            var rateY = self.Height / target.Height;

            var scale = 1.0 / (rateX > rateY ? rateX : rateY);

            return new Size(self.Width * scale, self.Height * scale);
        }

        //
        public static bool IsContains(this Size self, Size target)
        {
            return (target.Width <= self.Width && target.Height <= self.Height);
        }

        // 指定範囲内に収まるサイズを返す
        public static Size Limit(this Size self, Size target)
        {
            if (target.IsContains(self)) return self;

            return Uniformed(self, target);
        }

        // ほぼ同じサイズ？
        public static bool IsEqualMaybe(this Size self, Size target)
        {
            const double margin = 1.0;
            return Math.Abs(self.Width - target.Width) < margin && Math.Abs(self.Height - target.Height) < margin;
        }

    }

}
