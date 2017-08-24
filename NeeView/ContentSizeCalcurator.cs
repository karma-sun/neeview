// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// コンテンツ表示サイズ計算結果
    /// </summary>
    public class FixedContentSize
    {
        public List<Size> ContentSizeList { get; set; }
        public double ContentAngle { get; set; }
        public Thickness ContentsMargin { get; set; }
    }

    /// <summary>
    /// コンテンツ表示サイズ計算機
    /// </summary>
    public class ContentSizeCalcurator
    {
        #region Fields

        private ContentCanvas _contentCanvas;

        #endregion

        #region Constructors

        public ContentSizeCalcurator(ContentCanvas contentCanvas)
        {
            _contentCanvas = contentCanvas;
        }

        #endregion

        #region Properties

        private PageStretchMode StretchMode => _contentCanvas.StretchMode;
        private double ContentsSpace => _contentCanvas.ContentsSpace;
        private bool IsAutoRotate => _contentCanvas.IsAutoRotate;
        private Size ViewSize => _contentCanvas.ViewSize;

        #endregion

        #region Methods
        
        /// <summary>
        /// コンテンツ表示サイズを計算。
        /// 角度は自動回転から求める。
        /// </summary>
        /// <param name="source">元のコンテンツサイズ</param>
        /// <returns></returns>
        public FixedContentSize GetFixedContentSize(List<Size> source)
        {
            return GetFixedContentSize(source, GetAutoRotateAngle(source));
        }

        /// <summary>
        /// コンテンツ表示サイズを計算。
        /// </summary>
        /// <param name="source">元のコンテンツサイズ</param>
        /// <param name="angle">角度</param>
        /// <returns></returns>
        public FixedContentSize GetFixedContentSize(List<Size> source, double angle)
        {
            var dpi = Config.Current.Dpi;

            // 2ページ表示時は重なり補正を行う
            double offsetWidth = (source[0].Width > 0.5 && source[1].Width > 0.5) ? ContentsSpace / dpi.DpiScaleX : 0.0;

            // Viewにあわせたコンテンツサイズ
            var sizes = CalcContentSize(source, ViewSize.Width * dpi.DpiScaleX - offsetWidth, ViewSize.Height * dpi.DpiScaleY, angle);

            var result = new FixedContentSize();
            result.ContentAngle = angle;
            result.ContentsMargin = new Thickness(offsetWidth, 0, 0, 0);
            result.ContentSizeList = sizes.Select(e => new Size(e.Width / dpi.DpiScaleX, e.Height / dpi.DpiScaleY)).ToList();
            return result;
        }

        /// <summary>
        /// 自動回転角度を計算
        /// </summary>
        /// <param name="source">元のコンテンツサイズ</param>
        /// <returns></returns>
        public double GetAutoRotateAngle(List<Size> source)
        {
            var parameter = (AutoRotateCommandParameter)CommandTable.Current[CommandType.ToggleIsAutoRotate].Parameter;

            double angle = this.IsAutoRotateCondition(source)
                        ? parameter.AutoRotateType == AutoRotateType.Left ? -90.0 : 90.0
                        : 0.0;

            return angle;
        }

        //
        private bool IsAutoRotateCondition(List<Size> source)
        {
            if (!IsAutoRotate) return false;

            var margin = 0.1;
            var viewRatio = GetViewAreaAspectRatio();
            var contentRatio = GetContentAspectRatio(source);
            return viewRatio >= 1.0 ? contentRatio < (1.0 - margin) : contentRatio > (1.0 + margin);
        }

        //
        private double GetViewAreaAspectRatio()
        {
            return ViewSize.Width / ViewSize.Height;
        }

        //
        private double GetContentAspectRatio(List<Size> source)
        {
            var size = GetContentSize(source);
            return size.Width / size.Height;
        }

        //
        private Size GetContentSize(List<Size> source)
        {
            var c0 = source[0];
            var c1 = source[1];

            double rate0 = 1.0;
            double rate1 = 1.0;

            // 2ページ合わせたコンテンツの表示サイズを求める
            if (c1.IsZero())
            {
                return c0;
            }
            // オリジナルサイズ
            else if (this.StretchMode == PageStretchMode.None)
            {
                return new Size(c0.Width + c1.Width, Math.Max(c0.Height, c1.Height));
            }
            else
            {
                // どちらもImageでない
                if (c0.Width < 0.1 && c1.Width < 0.1)
                {
                    return new Size(1.0, 1.0);
                }

                if (c0.Width == 0) c0 = c1;
                if (c1.Width == 0) c1 = c0;

                // 高さを 高い方に合わせる
                if (c0.Height > c1.Height)
                {
                    rate1 = c0.Height / c1.Height;
                }
                else
                {
                    rate0 = c1.Height / c0.Height;
                }

                // 高さをあわせたときの幅の合計
                return new Size(c0.Width * rate0 + c1.Width * rate1, c0.Height * rate0);
            }
        }

        // ストレッチモードに合わせて各コンテンツのスケールを計算する
        private Size[] CalcContentSize(List<Size> source, double width, double height, double angle)
        {
            var c0 = source[0];
            var c1 = source[1];

            // オリジナルサイズ
            if (this.StretchMode == PageStretchMode.None)
            {
                return new Size[] { c0, c1 };
            }

            double rate0 = 1.0;
            double rate1 = 1.0;

            // 2ページ合わせたコンテンツの表示サイズを求める
            Size content;
            if (c1.IsZero())
            {
                content = c0;
            }
            else
            {
                // どちらもImageでない
                if (c0.Width < 0.1 && c1.Width < 0.1)
                {
                    return new Size[] { c0, c1 };
                }

                if (c0.Width == 0) c0 = c1;
                if (c1.Width == 0) c1 = c0;

                // 高さを 高い方に合わせる
                if (c0.Height > c1.Height)
                {
                    rate1 = c0.Height / c1.Height;
                }
                else
                {
                    rate0 = c1.Height / c0.Height;
                }

                // 高さをあわせたときの幅の合計
                content = new Size(c0.Width * rate0 + c1.Width * rate1, c0.Height * rate0);
            }

            // 回転反映
            {
                //var angle = 45.0;
                var rect = new Rect(content);
                var m = new Matrix();
                m.Rotate(angle);
                rect.Transform(m);

                content = new Size(rect.Width, rect.Height);
            }


            // ビューエリアサイズに合わせる場合のスケール
            double rateW = width / content.Width;
            double rateH = height / content.Height;

            // 拡大はしない
            if (this.StretchMode == PageStretchMode.Inside)
            {
                if (rateW > 1.0) rateW = 1.0;
                if (rateH > 1.0) rateH = 1.0;
            }
            // 縮小はしない
            else if (this.StretchMode == PageStretchMode.Outside)
            {
                if (rateW < 1.0) rateW = 1.0;
                if (rateH < 1.0) rateH = 1.0;
            }

            // 面積をあわせる
            if (this.StretchMode == PageStretchMode.UniformToSize)
            {
                var viewSize = width * height;
                var contentSize = content.Width * content.Height;
                var rate = Math.Sqrt(viewSize / contentSize);
                rate0 *= rate;
                rate1 *= rate;
            }
            // 高さを合わせる
            else if (this.StretchMode == PageStretchMode.UniformToVertical)
            {
                rate0 *= rateH;
                rate1 *= rateH;
            }
            // 枠いっぱいに広げる
            else if (this.StretchMode == PageStretchMode.UniformToFill)
            {
                if (rateW > rateH)
                {
                    rate0 *= rateW;
                    rate1 *= rateW;
                }
                else
                {
                    rate0 *= rateH;
                    rate1 *= rateH;
                }
            }
            // 枠に収めるように広げる
            else
            {
                if (rateW < rateH)
                {
                    rate0 *= rateW;
                    rate1 *= rateW;
                }
                else
                {
                    rate0 *= rateH;
                    rate1 *= rateH;
                }
            }

            var s0 = new Size(c0.Width * rate0, c0.Height * rate0);
            var s1 = new Size(c1.Width * rate1, c1.Height * rate1);
            return new Size[] { s0, s1 };
        }

        #endregion
    }
}
