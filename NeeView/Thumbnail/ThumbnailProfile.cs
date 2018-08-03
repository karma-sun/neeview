using NeeLaboratory;
using NeeLaboratory.ComponentModel;
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
    public class ThumbnailProfile : BindableBase
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
        [PropertyMember("@ParamThumbnailFormat", Tips = "@ParamThumbnailFormatTips")]
        public BitmapImageFormat Format { get; set; } = BitmapImageFormat.Jpeg;

        /// <summary>
        /// 画像品質
        /// </summary>
        private int _quality = 80;
        [PropertyRange("@ParamThumbnailQuality", 5, 100, TickFrequency = 5, Tips = "@ParamThumbnailQualityTips")]
        public int Quality
        {
            get { return _quality; }
            set { _quality = MathUtility.Clamp(value, 5, 100); }
        }

        [PropertyMember("@ParamThumbnailIsCacheEnabled", Tips = "@ParamThumbnailIsCacheEnabledTips")]
        public bool IsCacheEnabled { get; set; } = true;

        [PropertyMember("@ParamThumbnailPageCapacity", Tips = "@ParamThumbnailPageCapacityTips")]
        public int PageCapacity { get; set; } = 1000;

        [PropertyMember("@ParamThumbnailBookCapacity", Tips = "@ParamThumbnailBookCapacityTips")]
        public int BookCapacity { get; set; } = 200;

#if false
        private int _bannerWidth = 200;
        [PropertyRange("@ParamThumbnailBannerWidth", 0, 512, TickFrequency = 8, Tips = "@ParamThumbnailBannerWidthTips")]
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
        private int _thumbnailWidth = 64;
        [PropertyRange("@ParamThumbnailThumbnailWidth", 0, 256, TickFrequency = 8, Format = "{0}×{0}", Tips = "@ParamThumbnailThumbnailWidthTips")]
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


        private int _tileWidth = 128;
        [PropertyRange("@ParamThumbnailTileWidth", 64, 256, TickFrequency = 8, Format = "{0}×{0}", Tips = "@ParamThumbnailTileWidthTips")]
        public int TileWidth
        {
            get { return _tileWidth; }
            set
            {
                _tileWidth = MathUtility.Clamp(value, 64, 256);
                int width = _tileWidth;
                int height = _tileWidth;
                App.Current.Resources["TileWidth"] = (double)width;
                App.Current.Resources["TileHeight"] = (double)height;
            }
        }


        private bool _IsTileNameVisibled = true;
        [PropertyMember("@ParamThumbnailIsTileNameVisibled")]
        public bool IsTileNameVisibled
        {
            get { return _IsTileNameVisibled; }
            set { SetProperty(ref _IsTileNameVisibled, value); }
        }


        /// <summary>
        /// IsThumbnailPopup property.
        /// </summary>
        private bool _IsThumbnailPopup = true;
        [PropertyMember("@ParamThumbnailIsThumbnailPopup", Tips = "@ParamThumbnailIsThumbnailPopupTips")]
        public bool IsThumbnailPopup
        {
            get { return _IsThumbnailPopup; }
            set { if (_IsThumbnailPopup != value) { _IsThumbnailPopup = value;  } }
        }
#endif



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
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

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


            [Obsolete]
            [DataMember(EmitDefaultValue = false)]
            public int ThumbnailWidth { get; set; }

            [Obsolete]
            [DataMember(EmitDefaultValue = false)]
            public int BannerWidth { get; set; }

            [Obsolete]
            [DataMember(EmitDefaultValue = false)]
            public bool IsThumbnailPopup { get; set; }


            [OnDeserializing]
            private void Deserializing(StreamingContext context)
            {
                this.InitializePropertyDefaultValues();
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
            ////memento.ThumbnailWidth = this.ThumbnailWidth;
            ////memento.BannerWidth = this.BannerWidth;
            ////memento.TileWidth = this.TileWidth;
            ////memento.IsThumbnailPopup = this.IsThumbnailPopup;
            ////memento.IsTileNameVisibled = this.IsTileNameVisibled;
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
            ////this.ThumbnailWidth = memento.ThumbnailWidth;
            ////this.BannerWidth = memento.BannerWidth;
            ////this.TileWidth = memento.TileWidth;
            ////this.IsThumbnailPopup = memento.IsThumbnailPopup;
            ////this.IsTileNameVisibled = memento.IsTileNameVisibled;
        }


#pragma warning disable CS0612

        public void RestoreCompatible(Memento memento)
        {
            if (memento == null) return;

            // compatible before ver.32
            if (memento._Version < Config.GenerateProductVersionNumber(32, 0, 0))
            {
                SidePanelProfile.Current.ContentItemImageWidth = memento.ThumbnailWidth > 0 ? memento.ThumbnailWidth : 64;
                SidePanelProfile.Current.BannerItemImageWidth = memento.BannerWidth > 0 ? memento.BannerWidth : 200;
                SidePanelProfile.Current.ContentItemIsImagePopupEnabled = memento.IsThumbnailPopup;

                SidePanelProfile.Current.ValidatePanelListItemProfile();
            }
        }

#pragma warning restore CS0612

        #endregion

    }
}
