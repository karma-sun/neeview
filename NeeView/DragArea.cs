// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ドラッグエリア情報
    /// </summary>
    public class DragArea
    {
        // ビューエリアサイズ
        public Size View { get; private set; }

        // ターゲット領域
        public Rect Target { get; private set; }

        // ビューエリアオーバー情報
        public Rect Over { get; private set; }

        // コンストラクタ
        public DragArea(FrameworkElement view, FrameworkElement target)
        {
            View = new Size(view.ActualWidth, view.ActualHeight);
            Target = GetRealSize(target, view);

            Over = new Rect()
            {
                X = Target.Left < 0 ? Target.Left : 0,
                Y = Target.Left < Target.Top ? Target.Top : 0,
                Width = Target.Right > View.Width ? Target.Right - View.Width : 0,
                Height = Target.Bottom > View.Height ? Target.Bottom - View.Height : 0,
            };
        }


        // コントロールの表示RECTを取得
        public static Rect GetRealSize(FrameworkElement target, FrameworkElement parent)
        {
            Point[] pos = new Point[4];
            double width = target.ActualWidth;
            double height = target.ActualHeight;

            pos[0] = target.TranslatePoint(new Point(0, 0), parent);
            pos[1] = target.TranslatePoint(new Point(width, 0), parent);
            pos[2] = target.TranslatePoint(new Point(0, height), parent);
            pos[3] = target.TranslatePoint(new Point(width, height), parent);

            Point min = new Point(pos.Min(e => e.X), pos.Min(e => e.Y));
            Point max = new Point(pos.Max(e => e.X), pos.Max(e => e.Y));

            return new Rect(min, max);
        }
    }

}
