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
        static ThumbnailProfile() => Current = new ThumbnailProfile();
        public static ThumbnailProfile Current { get; }

        private ThumbnailProfile()
        {
        }

        /// <summary>
        /// BitmapFactoryでの画像生成モード
        /// </summary>
        public BitmapCreateMode CreateMode { get; } = BitmapCreateMode.HighQuality;


        /// <summary>
        /// サムネイル画像サイズ取得
        /// </summary>
        /// <param name="size">元画像サイズ</param>
        /// <returns></returns>
        public Size GetThumbnailSize(Size size)
        {
            var resolution = Config.Current.Thumbnail.Resolution;

            if (size.IsEmpty) return new Size(resolution, resolution);

            var pixels = resolution * resolution;

            var scale = Math.Sqrt(pixels / (size.Width * size.Height));

            var max = resolution * 2;
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
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

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


            [Obsolete, DataMember(EmitDefaultValue = false)] // ver.32
            public int ThumbnailWidth { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)] // ver.32
            public int BannerWidth { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)] // ver.32
            public bool IsThumbnailPopup { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext context)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.Thumbnail.IsCacheEnabled = IsCacheEnabled;
                config.Thumbnail.Format = Format;
                config.Thumbnail.Quality = Quality;
                config.Thumbnail.ThumbnailPageCapacity = PageCapacity;
                config.Thumbnail.ThumbnailBookCapacity = BookCapacity;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Format = Config.Current.Thumbnail.Format;
            memento.Quality = Config.Current.Thumbnail.Quality;
            memento.IsCacheEnabled = Config.Current.Thumbnail.IsCacheEnabled;
            memento.PageCapacity = Config.Current.Thumbnail.ThumbnailPageCapacity;
            memento.BookCapacity = Config.Current.Thumbnail.ThumbnailBookCapacity;
            return memento;
        }

        #endregion

    }
}
