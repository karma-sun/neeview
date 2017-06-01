// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class ThumbnailProfile
    {
        public static ThumbnailProfile Current { get; private set; }

        public ThumbnailProfile()
        {
            Current = this;
        }

        public int Quality
        {
            get { return _quality; }
            set { _quality = NVUtility.Clamp(value, 1, 100); }
        }
        private int _quality = 80;

        public bool IsCacheEnabled { get; set; } = true;
        public int PageCapacity { get; set; } = 1000;
        public int BookCapacity { get; set; } = 200;

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
        private int _bannerWidth = 200;

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(80)]
            [PropertyMember("サムネイル品質", Tips = "サムネイルのJpeg品質です。1-100で指定します")]
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
            this.Quality = memento.Quality;
            this.IsCacheEnabled = memento.IsCacheEnabled;
            this.PageCapacity = memento.PageCapacity;
            this.BookCapacity = memento.BookCapacity;
            this.BannerWidth = memento.BannerWidth;
        }
        #endregion

    }
}
