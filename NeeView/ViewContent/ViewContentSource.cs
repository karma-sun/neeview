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
        public Size Size => GetSize();

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
        /// 表示サイズ取得
        /// </summary>
        public Size GetSize()
        {
            var size = Page.Size;

            var trim = Config.Current.ImageTrim;
            if (trim.IsEnabled)
            {
                var width = Math.Max(size.Width - size.Width * (trim.Left + trim.Right), 0.0);
                var height = Math.Max(size.Height - size.Height * (trim.Top + trim.Bottom), 0.0);
                size = new Size(width, height);
            }

            switch (PagePart.PartSize)
            {
                case 0:
                    return new Size(0.0, size.Height);
                case 1:
                    return new Size(Math.Floor(size.Width * 0.5 + 0.4), size.Height);
                default:
                    return size;
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
            var crop = new Rect(0.0, 0.0, 1.0, 1.0);

            // トリミング
            var trim = Config.Current.ImageTrim;
            if (trim.IsEnabled)
            {
                var x = crop.X + trim.Left;
                var width = Math.Max(crop.Width - (trim.Left + trim.Right), 0.0);
                var y = crop.Y + trim.Top;
                var height = Math.Max(crop.Height - (trim.Top + trim.Bottom), 0.0);
                crop = new Rect(x, y, width, height);
            }

            // ページパートで領域分割
            crop = CropByPagePart(crop);

            // NOTE: ポリゴンの歪み補正
            crop.Offset(new Vector(-0.00001, -0.00001));

            return crop;
        }

        private Rect CropByPagePart(Rect rect)
        {
            switch (PagePart.PartSize)
            {
                case 0:
                    return new Rect(rect.X, rect.Y, 0.0, rect.Height);

                case 1:
                    bool isRightPart = PagePart.Position.Part == 0;
                    if (PagePart.PartOrder == PageReadOrder.LeftToRight) isRightPart = !isRightPart;
                    double half = rect.Width * 0.5;
                    double left = isRightPart ? rect.X + half : rect.X;
                    return new Rect(left, rect.Y, half, rect.Height);

                default:
                    return rect;
            }
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
