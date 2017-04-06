// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    public class ViewContentSource
    {
        // ページ
        // TODO: 不要にしたいが。難しいか。
        public Page Page { get; set; }


        // ソースコンテンツ
        public PageContent Content { get; set; }


        // コンテンツサイズ 
        public Size Size { get; set; }


        // ページの場所
        public PagePosition Position { get; set; }

        // ページサイズ
        public int PartSize { get; set; }

        // 方向
        public PageReadOrder ReadOrder { get; set; }

        // 有効
        public bool IsValid { get; set; }

        // コンストラクタ
        // Pageから作成
        public ViewContentSource(Page page, PagePosition position, int size, PageReadOrder readOrder)
        {
            Page = page;
            Content = page.Content;

            IsValid = Content.IsLoaded;

            Size = new Size(size == 2 ? Content.Size.Width : Math.Floor(Content.Size.Width * 0.5 + 0.4), Content.Size.Height);

            Position = position;
            PartSize = size;
            ReadOrder = readOrder;
        }


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
            if (PartSize == 0) return new Rect(0, -0.00001, 0, 0.99999);
            if (PartSize == 2) return new Rect(-0.00001, -0.00001, 0.99999, 0.99999);

            bool isRightPart = Position.Part == 0;
            if (ReadOrder == PageReadOrder.LeftToRight) isRightPart = !isRightPart;

            double half = Size.Width / Content.Size.Width;
            return isRightPart ? new Rect(0.99999 - half, -0.00001, half - 0.00001, 0.99999) : new Rect(-0.00001, -0.00001, half - 0.00001, 0.99999);
        }


        /// <summary>
        /// ページ用画像ブラシ作成
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public ImageBrush CreatePageImageBrush(BitmapSource bitmap)
        {
            var brush = new ImageBrush();
            brush.ImageSource = bitmap;
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.Stretch = Stretch.Fill;
            brush.TileMode = TileMode.None;
            brush.Viewbox = GetViewBox();
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
                return CreatePageImageBrush(Page.Thumbnail.CreateBitmap());
            }
            else if (reserver?.Thumbnail != null && reserver.Thumbnail.IsValid)
            {
                return CreatePageImageBrush(reserver.Thumbnail.CreateBitmap());
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
            }
        }
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
        Thumbnail,
    }

    /// <summary>
    /// 表示コンテンツソースの種類
    /// </summary>
    public enum ViewSourceType
    {
        None, // 無し
        Content, // コンテンツ
    }

    /// <summary>
    /// 表示コンテンツソース
    /// </summary>
    public class ViewSource
    {
        public ViewSourceType Type { get; set; }
        public List<ViewContentSource> Sources { get; set; }
        public int Direction { get; set; }
    }
}
