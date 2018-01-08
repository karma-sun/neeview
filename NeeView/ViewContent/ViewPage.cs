// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    // 表示コンテンツソース 1ページ分
    public class ViewPage
    {
        #region Constructors

        // コンストラクタ
        // Pageから作成
        public ViewPage(Page page, PagePart pagePart)
        {
            Page = page;
            Content = page.Content;

            IsValid = Content.IsLoaded;

            Size = new Size(pagePart.PartSize == 2 ? Page.Width : Math.Floor(Page.Width * 0.5 + 0.4), Page.Height);

            this.PagePart = pagePart;
        }

        #endregion

        #region Properties

        // ページ
        // TODO: 不要にしたいが。難しいか。
        public Page Page { get; }

        // ソースコンテンツ
        public PageContent Content { get; }

        // コンテンツサイズ 
        public Size Size { get; }

        // ページパーツ
        public PagePart PagePart { get; }

        // 生成時点での有効判定
        public bool IsValid { get; }

        #endregion

        #region Methods

        /// <summary>
        /// TODO: PageContentで実装すべきか
        /// </summary>
        /// <returns></returns>
        public ViewContentType GetContentType()
        {
            // テキスト表示
            if (Content.PageMessage != null)
            {
                return ViewContentType.Message;
            }
            // 仮表示
            else if (!Content.IsLoaded)
            {
                return ViewContentType.Thumbnail;
            }
            // PDF
            else if (Content is PdfContetnt)
            {
                return ViewContentType.Pdf;
            }
            // アニメーション
            else if (Content is AnimatedContent)
            {
                return ViewContentType.Anime;
            }
            // 画像
            else if (Content is BitmapContent)
            {
                return ViewContentType.Bitmap;
            }
            else
            {
                return ViewContentType.None;
            }
        }

        /// <summary>
        /// ViewBox取得.
        /// ページ分割対応.
        /// ポリゴン表示誤吸収のための補正付き
        /// </summary>
        /// <returns></returns>
        public Rect GetViewBox()
        {
            if (PagePart.PartSize == 0) return new Rect(0, -0.00001, 0, 0.99999);
            if (PagePart.PartSize == 2) return new Rect(-0.00001, -0.00001, 0.99999, 0.99999);

            bool isRightPart = PagePart.Position.Part == 0;
            if (PagePart.PartOrder == PageReadOrder.LeftToRight) isRightPart = !isRightPart;

            double half = Size.Width / Page.Width;
            return isRightPart ? new Rect(0.99999 - half, -0.00001, half - 0.00001, 0.99999) : new Rect(-0.00001, -0.00001, half - 0.00001, 0.99999);
        }

        /// <summary>
        /// ViewBoxを適用したBitmapのサイズを取得.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public Size GetViewBitmapSize(BitmapSource bitmap)
        {
            return new Size(Math.Truncate(bitmap.PixelWidth * GetViewBox().Width + 0.1), bitmap.PixelHeight);
        }


        /// <summary>
        /// ページ用画像ブラシ作成
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public ImageBrush CreatePageImageBrush(BitmapSource bitmap, bool isStretch)
        {
            var brush = new ImageBrush();
            brush.ImageSource = bitmap;
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.Stretch = isStretch ? Stretch.Fill : Stretch.None;
            brush.TileMode = TileMode.None;
            brush.Viewbox = GetViewBox();
            brush.Freeze();

            return brush;
        }

        /// <summary>
        /// ページ用画像ブラシの画像を差し替えて複製
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public ImageBrush ClonePageImageBrush(ImageBrush source, BitmapSource bitmap)
        {
            var brush = source.Clone();
            brush.ImageSource = bitmap;
            brush.Freeze();

            return brush;
        }

        /// <summary>
        /// サムネイル作成
        /// </summary>
        /// <returns></returns>
        public Brush CreateThumbnailBrush(ViewContentReserver reserver)
        {
            if (Page.Thumbnail.IsValid)
            {
                return CreatePageImageBrush(Page.Thumbnail.BitmapSource, true);
            }
            else if (reserver != null)
            {
                return reserver.Brush;
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
            }
        }

        #endregion
    }

    /// <summary>
    /// ViewContentの種類
    /// </summary>
    public enum ViewContentType
    {
        None,
        Message,
        Bitmap,
        Anime,
        Pdf,
        Thumbnail,
    }
}
