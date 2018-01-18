// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
                Y = Target.Top < 0 ? Target.Top : 0,
                Width = Target.Width > View.Width ? Target.Width - View.Width : 0,
                Height = Target.Height > View.Height ? Target.Height - View.Height : 0,
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


        // エリアサイズ内に座標を収める
        public Point SnapView(Point pos)
        {
            return (Point)SnapView((Vector)pos);
        }

        /// <summary>
        ///  エリアサイズ内に座標を収める
        /// </summary>
        /// <param name="pos">コンテンツ中心座標</param>
        /// <returns>補正された中心座標</returns>
        public Vector SnapView(Vector pos)
        {
            double margin = 1.0;

            // ウィンドウサイズ変更直後はrectのスクリーン座標がおかしい可能性があるのでPositionから計算しなおす
            var rect = new Rect()
            {
                X = pos.X - this.Target.Width * 0.5 + this.View.Width * 0.5,
                Y = pos.Y - this.Target.Height * 0.5 + this.View.Height * 0.5,
                Width = this.Target.Width,
                Height = this.Target.Height,
            };

            if (rect.Width <= this.View.Width + margin)
            {
                pos.X = 0;
            }
            else
            {
                if (rect.Left > 0)
                {
                    pos.X -= rect.Left;
                }
                else if (rect.Right < this.View.Width)
                {
                    pos.X += this.View.Width - rect.Right;
                }
            }

            if (rect.Height <= this.View.Height + margin)
            {
                pos.Y = 0;
            }
            else
            {
                if (rect.Top > 0)
                {
                    pos.Y -= rect.Top;
                }
                else if (rect.Bottom < this.View.Height)
                {
                    pos.Y += this.View.Height - rect.Bottom;
                }
            }

            return pos;
        }

    }
}
