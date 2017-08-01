﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        public BitmapImageFormat Format { get; set; } = BitmapImageFormat.Jpeg;

        /// <summary>
        /// 画像品質
        /// </summary>
        private int _quality = 80;
        public int Quality
        {
            get { return _quality; }
            set { _quality = NVUtility.Clamp(value, 1, 100); }
        }

        public bool IsCacheEnabled { get; set; } = true;
        public int PageCapacity { get; set; } = 1000;
        public int BookCapacity { get; set; } = 200;

        private int _bannerWidth = 200;
        public int BannerWidth
        {
            get { return _bannerWidth; }
            set
            {
                _bannerWidth = NVUtility.Clamp(value, 32, 512);
                int bannerWidth = _bannerWidth;
                int bannerHeight = _bannerWidth / 4;
                App.Current.Resources["BannerWidth"] = (double)bannerWidth;
                App.Current.Resources["BannerHeight"] = (double)bannerHeight;
            }
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
            setting.ProcessImageSettings = new ProcessImageSettings() { HybridMode = HybridScaleMode.Turbo };
            return setting;
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(BitmapImageFormat.Jpeg)]
            [PropertyEnum("サムネイルフォーマット", Tips = "サムネイル画像のフォーマットです。Pngは劣化がなく最高品質ですが、Jpegより多くのメモリを消費します")]
            public BitmapImageFormat Format { get; set; } = BitmapImageFormat.Jpeg;

            [DataMember, DefaultValue(80)]
            [PropertyMember("サムネイル品質", Tips = "サムネイルフォーマットがJpegの場合の品質です。1-100で指定します")]
            public int Quality { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("サムネイルキャッシュを使用する", Tips = "ブックサムネイルをキャッシュします。キャッシュファイルはCache.dbです")]
            public bool IsCacheEnabled { get; set; }

            [DataMember, DefaultValue(1000)]
            [PropertyMember("ページサムネイル容量", Tips = "ページサムネイル保持枚数です。ブックを閉じると全てクリアされます")]
            public int PageCapacity { get; set; }

            [DataMember, DefaultValue(200)]
            [PropertyMember("ブックサムネイル容量", Tips = "フォルダーリスト等でのサムネイル保持枚数です")]
            public int BookCapacity { get; set; }

            [DataMember, DefaultValue(200)]
            [PropertyMember("バナーサイズ", Tips = "バナーの横幅です。縦幅は横幅の1/4になります。\nサムネイル画像を流用しているため、大きいサイズほど画像が荒くなります")]
            public int BannerWidth { get; set; }
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
            memento.BannerWidth = this.BannerWidth;
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
            this.BannerWidth = memento.BannerWidth;
        }
        #endregion

    }
}
