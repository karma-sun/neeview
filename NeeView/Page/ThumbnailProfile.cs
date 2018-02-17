// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory;
using NeeView.Windows.Property;
using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class ThumbnailProfile
    {
        public static ThumbnailProfile Current { get; private set; }

        public ThumbnailProfile()
        {
            Current = this;
        }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        public double Size { get; } = 256;

        /// <summary>
        /// BitmapFactoryでの画像生成モード
        /// </summary>
        public BitmapCreateMode CreateMode { get; } = BitmapCreateMode.HighQuality;

        /// <summary>
        /// 画像フォーマット
        /// </summary>
        [PropertyMember("サムネイル画像フォーマット", Tips = "Pngは高品質ですが、Jpegより多くのメモリを消費します。")]
        public BitmapImageFormat Format { get; set; } = BitmapImageFormat.Jpeg;

        /// <summary>
        /// 画像品質
        /// </summary>
        private int _quality = 80;
        [PropertyRange("サムネイル品質", 5, 100, TickFrequency = 5, Tips = "サムネイル画像フォーマットがJpegの場合の品質です。")]
        public int Quality
        {
            get { return _quality; }
            set { _quality = MathUtility.Clamp(value, 5, 100); }
        }

        [PropertyMember("サムネイルキャッシュを使用する", Tips = "ブックサムネイルをキャッシュします。")]
        public bool IsCacheEnabled { get; set; } = true;

        [PropertyMember("ページサムネイル容量", Tips = "メモリ上のページサムネイルの保持枚数です。ブックを閉じると全て破棄されます。")]
        public int PageCapacity { get; set; } = 1000;

        [PropertyMember("ブックサムネイル容量", Tips = "フォルダーリスト等でのメモリ上のサムネイル保持枚数です。")]
        public int BookCapacity { get; set; } = 200;

        private int _bannerWidth = 200;
        [PropertyRange("バナーサイズ", 0, 512, TickFrequency = 8, Tips = "パネルのバナー表示での画像の横幅です。縦幅は横幅の1/4になります。サムネイル画像を流用しているため大きいサイズほど画像が荒くなります。")]
        public int BannerWidth
        {
            get { return _bannerWidth; }
            set
            {
                _bannerWidth = MathUtility.Clamp(value, 0, 512);
                int bannerWidth = _bannerWidth;
                int bannerHeight = _bannerWidth / 4;
                App.Current.Resources["BannerWidth"] = (double)bannerWidth;
                App.Current.Resources["BannerHeight"] = (double)bannerHeight;
            }
        }

        /// <summary>
        /// ThumbnailWidth property.
        /// </summary>
        private int _thumbnailWidth = 48;
        [PropertyRange("ブックサムネイルサイズ", 0, 256, TickFrequency = 8, Format = "{0}×{0}", Tips = "パネルのコンテンツ表示でのサムネイルサイズです。")]
        public int ThumbnailWidth
        {
            get { return _thumbnailWidth; }
            set
            {
                _thumbnailWidth = MathUtility.Clamp(value, 0, 256);
                int width = _thumbnailWidth;
                int height = _thumbnailWidth + 10;
                App.Current.Resources["ThumbnailWidth"] = (double)width;
                App.Current.Resources["ThumbnailHeight"] = (double)height;
            }
        }

        /// <summary>
        /// IsThumbnailPopup property.
        /// </summary>
        private bool _IsThumbnailPopup = true;
        [PropertyMember("ブックサムネイルのポップアップ", Tips = "サムネイルにカーソルを合わせるとポップアップで大きめのサムネイル画像が表示されます。")]
        public bool IsThumbnailPopup
        {
            get { return _IsThumbnailPopup; }
            set { if (_IsThumbnailPopup != value) { _IsThumbnailPopup = value;  } }
        }



        /// <summary>
        /// サムネイル画像サイズ取得
        /// </summary>
        /// <param name="size">元画像サイズ</param>
        /// <returns></returns>
        public Size GetThumbnailSize(Size size)
        {
            if (size.IsEmpty) return new Size(Size, Size);

            var pixels = Size * Size;

            var scale = Math.Sqrt(pixels / (size.Width * size.Height));

            var max = Size * 2;
            if (size.Width * scale > max) scale = max / size.Width;
            if (size.Height * scale > max) scale = max / size.Height;
            if (scale > 1.0) scale = 1.0;

            var thumbnailSize = new Size(size.Width * scale, size.Height * scale);

            return thumbnailSize;
        }

        //
        public BitmapCreateSetting CreateBitmapCreateSetting()
        {
            var setting = new BitmapCreateSetting();
            setting.Mode = this.CreateMode;
            setting.ProcessImageSettings = new ProcessImageSettings()
            {
                HybridMode = HybridScaleMode.Turbo,
                MatteColor = System.Drawing.Color.White,
            };
            return setting;
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(BitmapImageFormat.Jpeg)]
            public BitmapImageFormat Format { get; set; } = BitmapImageFormat.Jpeg;

            [DataMember, DefaultValue(80)]
            public int Quality { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsCacheEnabled { get; set; }

            [DataMember, DefaultValue(1000)]
            public int PageCapacity { get; set; }

            [DataMember, DefaultValue(200)]
            public int BookCapacity { get; set; }

            [DataMember, DefaultValue(48)]
            public int ThumbnailWidth { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsThumbnailPopup { get; set; }

            [DataMember, DefaultValue(200)]
            public int BannerWidth { get; set; }


            [OnDeserializing]
            private void Deserializing(StreamingContext context)
            {
                ThumbnailWidth = 48;
                BannerWidth = 200;
                IsThumbnailPopup = true;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Format = this.Format;
            memento.Quality = this.Quality;
            memento.IsCacheEnabled = this.IsCacheEnabled;
            memento.PageCapacity = this.PageCapacity;
            memento.BookCapacity = this.BookCapacity;
            memento.ThumbnailWidth = this.ThumbnailWidth;
            memento.BannerWidth = this.BannerWidth;
            memento.IsThumbnailPopup = this.IsThumbnailPopup;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.Format = memento.Format;
            this.Quality = memento.Quality;
            this.IsCacheEnabled = memento.IsCacheEnabled;
            this.PageCapacity = memento.PageCapacity;
            this.BookCapacity = memento.BookCapacity;
            this.ThumbnailWidth = memento.ThumbnailWidth;
            this.BannerWidth = memento.BannerWidth;
            this.IsThumbnailPopup = memento.IsThumbnailPopup;
        }
        #endregion

    }
}
