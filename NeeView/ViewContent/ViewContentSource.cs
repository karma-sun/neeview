using NeeView.Media.Imaging;
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
        // コンストラクタ
        // Pageから作成
        public ViewContentSource(Page page, PagePart pagePart)
        {
            Page = page;
            PagePart = pagePart;

            Content = page.GetContentClone();
            IsValid = Content.IsLoaded;
        }

        public ViewContentSource(Page page, PagePart pagePart, bool isDummy) : this(page, pagePart)
        {
            IsDummy = isDummy;
        }

        // ページ
        // TODO: 不要にしたいが。難しいか。
        public Page Page { get; }

        /// <summary>
        /// ページコンテンツ。
        /// Cloneなので不変。編集不可。
        /// </summary>
        public PageContent Content { get; }

        // コンテンツサイズ 
        public Size Size => PagePart.PartSize == 2 ? Page.Size : new Size(Math.Floor(Page.Width * 0.5 + 0.4), Page.Height);

        // ページパーツ
        public PagePart PagePart { get; }

        // 分割？
        public bool IsHalf => this.PagePart.PartSize == 1;

        // 生成時点での有効判定
        public bool IsValid { get; }

        /// <summary>
        /// メディア用。最後から再生開始
        /// </summary>
        public bool IsLastStart => Content is MediaContent content ? content.IsLastStart : false;

        public bool IsDummy { get; }

        /// <summary>
        /// TODO: PageContentで実装すべきか
        /// </summary>
        /// <returns></returns>
        public ViewContentType GetContentType()
        {
            // ダミー
            if (IsDummy)
            {
                return ViewContentType.Dummy;
            }
            // テキスト表示
            else if (Content.PageMessage != null)
            {
                return ViewContentType.Message;
            }
            // アーカイブ
            else if (Content is ArchiveContent)
            {
                return ViewContentType.Archive;
            }
            // 仮表示
            else if (!Content.IsViewReady)
            {
                return ViewContentType.Reserve;
            }
            // PDF
            else if (Content is PdfContent)
            {
                return ViewContentType.Pdf;
            }
            // アニメーション
            else if (Content is AnimatedContent)
            {
                return ViewContentType.Anime;
            }
            // メディア
            else if (Content is MediaContent)
            {
                return ViewContentType.Media;
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
        /// <param name="image"></param>
        /// <returns></returns>
        public Size GetViewBitmapSize(ImageSource image)
        {
            return new Size(Math.Truncate(image.GetPixelWidth() * GetViewBox().Width + 0.1), image.GetPixelHeight());
        }


        /// <summary>
        /// ページ用画像ブラシ作成
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public ImageBrush CreatePageImageBrush(ImageSource bitmap, bool isStretch)
        {
            var brush = new ImageBrush();
            brush.ImageSource = bitmap;
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.Stretch = isStretch ? Stretch.Fill : Stretch.None;
            brush.TileMode = TileMode.None;
            brush.Viewbox = GetViewBox();
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            return brush;
        }

        /// <summary>
        /// ページ用画像ブラシの画像を差し替えて複製
        /// </summary>
        /// <param name="source"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public ImageBrush ClonePageImageBrush(ImageBrush source, ImageSource image)
        {
            var brush = source.Clone();
            brush.ImageSource = image;
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            return brush;
        }

        /// <summary>
        /// 予備ブラシ作成
        /// </summary>
        /// <returns></returns>
        public Brush CreateReserveBrush(ViewContentReserver reserver)
        {
            if (reserver != null)
            {
                return reserver.Brush;
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
        Dummy,
        Message,
        Bitmap,
        Anime,
        Media,
        Pdf,
        Archive,
        Reserve,
    }

}
