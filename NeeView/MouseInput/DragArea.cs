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

        /// <summary>
        /// ビューエリアサイズ
        /// </summary>
        public Size View { get; private set; }

        /// <summary>
        /// ターゲットエリア
        /// </summary>
        public Rect Target { get; private set; }

        /// <summary>
        /// ビューエリアオーバー情報.
        /// X,Y はターゲットがビューエリアからマイナスにはみ出している場合のみその値を記憶する。
        /// Width,Height はターゲットサイズがビューエリアサイズを超える差分を記憶する。
        /// </summary>
        public Rect Over { get; private set; }

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
            return (Point)SnapView((Vector)pos, false);
        }

        /// <summary>
        ///  エリアサイズ内に座標を収める
        /// </summary>
        /// <param name="pos">コンテンツ中心座標</param>
        /// <param name="centered">範囲内に収まるときは中央に配置</param>
        /// <returns>補正された中心座標</returns>
        public Vector SnapView(Vector pos, bool centered)
        {
            const double margin = 1.0;

            // ウィンドウサイズ変更直後はrectのスクリーン座標がおかしい可能性があるのでPositionから計算しなおす
            var rect = new Rect()
            {
                X = pos.X - this.Target.Width * 0.5 + this.View.Width * 0.5,
                Y = pos.Y - this.Target.Height * 0.5 + this.View.Height * 0.5,
                Width = this.Target.Width,
                Height = this.Target.Height,
            };

            var minX = this.View.Width * -0.5 + rect.Width * 0.5;
            var maxX = minX + this.View.Width - rect.Width;

            if (rect.Width <= this.View.Width + margin)
            {
                if (centered)
                {
                    pos.X = 0.0;
                }
                else if (rect.Left < 0)
                {
                    pos.X = minX;
                }
                else if (rect.Right > this.View.Width)
                {
                    pos.X = maxX;
                }
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

            var minY = this.View.Height * -0.5 + rect.Height * 0.5;
            var maxY = minY + this.View.Height - rect.Height;

            if (rect.Height <= this.View.Height + margin)
            {
                if (centered)
                {
                    pos.Y = 0.0;
                }
                else if (rect.Top < 0)
                {
                    pos.Y = minY;
                }
                else if (rect.Bottom > this.View.Height)
                {
                    pos.Y = maxY;
                }
            }
            else
            {
                if (rect.Top > 0)
                {
                    pos.Y = minY;
                }
                else if (rect.Bottom < this.View.Height)
                {
                    pos.Y = maxY;
                }
            }

            return pos;
        }

    }
}
