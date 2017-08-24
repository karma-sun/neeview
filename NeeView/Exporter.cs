// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// エクスポートタイプ
    /// </summary>
    public enum ExportType
    {
        Single, // 単ページ
        Double, // 連結ページ
    }


    /// <summary>
    /// ページ画像
    /// </summary>
    public class PageVisual
    {
        public Page Page { get; set; } // ページ
        public FrameworkElement VisualContent { get; set; } // 描写用コンテンツ
        public string Name { get; set; } // 名前
        public string DefaultExtension { get; set; } // デフォルト拡張子

        //
        public PageVisual()
        {
        }

        //
        public PageVisual(Page page)
        {
            Page = page;
            Name = System.IO.Path.GetFileName(page.FileName);
            DefaultExtension = System.IO.Path.GetExtension(page.FileName).ToLower();
        }

        // コンテンツが読まれていなければ読み込んでからサムネイルを作成する
        public async Task<FrameworkElement> CreateVisualContentAsync(Size maxSize, bool isShadowEffect)
        {
            if (Page == null) return null;

            await Page.LoadThumbnailAsync(QueueElementPriority.Top);

            BitmapSource source = Page.Thumbnail.BitmapSource;

            return CreateVisualContent(source, new Size(Page.Width, Page.Height), maxSize, isShadowEffect);
        }


        // サムネイル作成
        private static FrameworkElement CreateVisualContent(BitmapSource bitmapSource, Size sourceSize, Size maxSize, bool isShadowEffect)
        {
            if (bitmapSource == null) return null;

            var image = new Image();
            image.Source = bitmapSource;

            var scaleX = sourceSize.Width > maxSize.Width ? maxSize.Width / sourceSize.Width : 1.0;
            var scaleY = sourceSize.Height > maxSize.Height ? maxSize.Height / sourceSize.Height : 1.0;
            var scale = scaleX > scaleY ? scaleY : scaleX;

            image.Width = sourceSize.Width * scale;
            image.Height = sourceSize.Height * scale;
            image.Stretch = Stretch.Fill;
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            if (isShadowEffect)
            {
                image.Effect = new DropShadowEffect()
                {
                    Opacity = 0.5,
                    ShadowDepth = 2,
                    RenderingBias = RenderingBias.Quality
                };
            }

            return image;
        }
    }

    /// <summary>
    /// エクスポーター
    /// </summary>
    public class Exporter
    {
        // メッセージ処理：ファイル出力
        public bool ShowDialog()
        {
            var dialog = new SaveWindow(this);
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            return (result == true);
        }



        // 単ページ
        public PageVisual SingleImage { get; set; }

        // 連結ページ
        public PageVisual DoubleImage { get; set; }

        // 設定からどちらのページを使用するか選択して返す
        public PageVisual CurrentImage => (ExportType == ExportType.Double && DoubleImage != null) ? DoubleImage : SingleImage;

        // 生成された画像
        public BitmapSource BitmapSource { get; set; }

        // そのまま出力するかのフラグ
        public bool IsHintClone { get; set; } = ExporterProfile.Current.IsHintCloneDefault;

        // 背景ブラシ
        public Brush Background { get; set; }
        public Brush BackgroundFront { get; set; }

        // 背景の出力フラグ
        public bool IsHintBackground { get; set; } = false;

        // 出力パス
        public string Path { get; set; }

        // 出力タイプ
        public ExportType ExportType { get; set; } = ExportType.Single;


        // 初期化 
        public void Initialize(List<Page> pages, PageReadOrder order, string doubleImageName)
        {
            if (pages.Count <= 0) throw new ArgumentException("pages empty.", "pages");

            // single
            {
                SingleImage = new PageVisual();
                SingleImage.Page = pages[0];
                SingleImage.Name = System.IO.Path.GetFileName(pages[0].FileName);
                SingleImage.DefaultExtension = System.IO.Path.GetExtension(SingleImage.Name).ToLower();

                // visual
                var image = new Image();
                image.Source = (pages[0].Content as BitmapContent)?.BitmapSource;
                if (image.Source == null) throw new ArgumentException("pages[0] don't hage BitmapSource", "pages");
                image.Width = SingleImage.Page.Width;
                image.Height = SingleImage.Page.Height;
                image.Stretch = Stretch.Fill;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

                SingleImage.VisualContent = image;
            }

            // double
            if (pages.Count > 1)
            {
                DoubleImage = new PageVisual();
                DoubleImage.Name = doubleImageName;
                DoubleImage.DefaultExtension = ".png";

                // visual
                var stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.SnapsToDevicePixels = true;

                var maxHeight = pages.Aggregate(0.0, (max, e) => (max > e.Height) ? max : e.Height);

                stackPanel.Width = 0;
                stackPanel.Height = maxHeight;

                List<Page> sortedPages = (order == PageReadOrder.RightToLeft) ? pages.Reverse<Page>().ToList() : pages;
                foreach (var page in sortedPages)
                {
                    var image = new Image();
                    image.Source = (page.Content as BitmapContent).BitmapSource;
                    if (image.Source == null) throw new ArgumentException("any pages don't hage BitmapSource", "pages");
                    image.Width = page.Width * (maxHeight / page.Height);
                    image.Height = maxHeight;
                    image.Stretch = Stretch.Fill;
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                    stackPanel.Children.Add(image);

                    if (page == sortedPages[0])
                    {
                        stackPanel.Width += image.Width;
                    }
                    else
                    {
                        image.Margin = new Thickness(-1, 0, 0, 0);
                        stackPanel.Width += image.Width - 1;
                    }
                }

                DoubleImage.VisualContent = stackPanel;
            }
        }


        // 画像更新
        public void UpdateBitmapSource()
        {
            if (CurrentImage.VisualContent == null)
            {
                BitmapSource = null;
                return;
            }

            var canvas = new Canvas();

            var content = CurrentImage.VisualContent;
            canvas.Children.Add(content);

            if (IsHintBackground)
            {
                canvas.Background = this.Background;

                var rectangle = new Rectangle();
                rectangle.Width = content.Width;
                rectangle.Height = content.Height;
                rectangle.Fill = this.BackgroundFront;
                RenderOptions.SetBitmapScalingMode(rectangle, BitmapScalingMode.HighQuality);
                canvas.Children.Insert(0, rectangle);
            }

            canvas.Width = content.Width;
            canvas.Height = content.Height;

            // ビューツリー外でも正常にレンダリングするようにする処理
            canvas.Measure(new Size(canvas.Width, canvas.Height));
            canvas.Arrange(new Rect(new Size(canvas.Width, canvas.Height)));
            canvas.UpdateLayout();

            double dpi = 96.0;
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(canvas);

            canvas.Children.Clear(); // コンテンツ開放

            BitmapSource = bmp;
        }


        // クローン保存できる設定かチェックする
        public bool CanClone(bool checkFilenameExt)
        {
            if (IsHintClone) return true;

            bool result = true;

            result = result && ExportType == ExportType.Single && SingleImage != null; // 単ページ設定
            result = result && !IsHintBackground; // 背景を含めない

            if (checkFilenameExt)
            {
                result = result && SingleImage.DefaultExtension == System.IO.Path.GetExtension(Path)?.ToLower();// 拡張子が同じ
            }

            return result;
        }

        // 画像出力
        public void Export()
        {
            // デフォルト設定上書き
            ExporterProfile.Current.IsHintCloneDefault = IsHintClone;
            ExporterProfile.Current.ExportFolder = System.IO.Path.GetDirectoryName(Path);

            // ファイルのクローン
            if (CanClone(true))
            {
                SingleImage.Page.Entry.ExtractToFile(Path, true);
            }
            // 再レンダリング
            else
            {
                UpdateBitmapSource();

                if (BitmapSource == null) throw new ApplicationException("Null BitmapSource");

                using (FileStream stream = new FileStream(Path, FileMode.Create))
                {
                    // 出力ファイル名からフォーマットを決定する
                    if (System.IO.Path.GetExtension(Path).ToLower() == ".png")
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(BitmapSource));
                        encoder.Save(stream);
                    }
                    else
                    {
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.QualityLevel = ExporterProfile.Current.QualityLevel;
                        encoder.Frames.Add(BitmapFrame.Create(BitmapSource));
                        encoder.Save(stream);
                    }
                }
            }
        }

        #region Memento (Obsolete)

        //
        [Obsolete, DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsHintCloneDefault { get; set; }

            [DataMember]
            public int QualityLevel { get; set; }

            [DataMember]
            public string ExportFolder { get; set; }

            [DataMember]
            public bool IsEnableExportFolder { get; set; }

            //
            private void Constructor()
            {
                IsHintCloneDefault = true;
                QualityLevel = 80;
                ExportFolder = null;
                IsEnableExportFolder = true;
            }

            //
            public Memento()
            {
                Constructor();
            }

            //
            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }
        }

#pragma warning disable CS0612

        //
        public static void RestoreCompatible(Memento memento)
        {
            ExporterProfile.Current.IsHintCloneDefault = memento.IsHintCloneDefault;
            ExporterProfile.Current.QualityLevel = memento.QualityLevel;
            ExporterProfile.Current.ExportFolder = memento.ExportFolder;
            ExporterProfile.Current.IsEnableExportFolder = memento.IsEnableExportFolder;
        }

#pragma warning restore CS0612

        #endregion
    }
}
