using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    public class Config : BindableBase
    {
        public static Config Current { get; } = new Config();


        public SystemConfig System { get; set; } = new SystemConfig();

        public StartUpConfig StartUp { get; set; } = new StartUpConfig();

        public PerformanceConfig Performance { get; set; } = new PerformanceConfig();

        public ImageConfig Image { get; set; } = new ImageConfig();

        public ArchiveConfig Archive { get; set; } = new ArchiveConfig();

        public SusieConfig Susie { get; set; } = new SusieConfig();

        public HistoryConfig History { get; set; } = new HistoryConfig();

        public BookmarkConfig Bookmark { get; set; } = new BookmarkConfig();

        public PagemarkConfig Pagemark { get; set; } = new PagemarkConfig();

        public WindowConfig Window { get; set; } = new WindowConfig();

        public LayoutConfig Layout { get; set; } = new LayoutConfig();

        public ThumbnailConfig Thumbnail { get; set; } = new ThumbnailConfig();

        public SlideShowConfig SlideShow { get; set; } = new SlideShowConfig();

        public EffectConfig Effect { get; set; } = new EffectConfig();

        public ImageCustomSizeConfig ImageCustomSize { get; set; } = new ImageCustomSizeConfig();

        public ImageDotKeepConfig ImageDotKeep { get; set; } = new ImageDotKeepConfig();

        public ImageGridConfig ImageGridConfig { get; set; } = new ImageGridConfig();

        public ImageResizeFilterConfig ImageResizeFilter { get; set; } = new ImageResizeFilterConfig();


        public CommandConfig Command { get; set; } = new CommandConfig();

        public ScriptConfig Script { get; set; } = new ScriptConfig();

        /// <summary>
        /// Configプロパティを上書き
        /// </summary>
        public void Merge(Config config)
        {
            if (config == null) throw new ArgumentNullException();
            ObjectTools.Merge(this, config);
        }
    }


    public class ImageCustomSizeConfig : BindableBase
    {
        private bool _IsEnabled;
        private bool _IsUniformed;
        private Size _Size = new Size(256, 256);


        /// <summary>
        /// 指定サイズ有効
        /// </summary>
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { SetProperty(ref _IsEnabled, value); }
        }

        /// <summary>
        /// カスタムサイズ
        /// </summary>
        public Size Size
        {
            get { return _Size; }
            set
            {
                if (SetProperty(ref _Size, value))
                {
                    RaisePropertyChanged(nameof(Width));
                    RaisePropertyChanged(nameof(Height));
                }
            }
        }

        /// <summary>
        /// カスタムサイズ：横幅
        /// </summary>
        [PropertyRange("@ParamPictureCustomWidth", 16, 4096)]
        [JsonIgnore, PropertyMapIgnore]
        public int Width
        {
            get { return (int)_Size.Width; }
            set { if (value != _Size.Width) { Size = new Size(value, _Size.Height); } }
        }

        /// <summary>
        /// カスタムサイズ：縦幅
        /// </summary>
        [PropertyRange("@ParamPictureCustomHeight", 16, 4096)]
        [JsonIgnore, PropertyMapIgnore]
        public int Height
        {
            get { return (int)_Size.Height; }
            set { if (value != _Size.Height) { Size = new Size(_Size.Width, value); } }
        }

        /// <summary>
        /// 縦横比を固定する
        /// </summary>
        [PropertyMember("@ParamPictureCustomLockAspect")]
        public bool IsUniformed
        {
            get { return _IsUniformed; }
            set { SetProperty(ref _IsUniformed, value); }
        }

        /// <summary>
        /// ハッシュ値の計算
        /// </summary>
        /// <returns></returns>
        public int GetHashCodde()
        {
            var hash = (_IsEnabled.GetHashCode() << 30) ^ (_IsUniformed.GetHashCode() << 29) ^ _Size.GetHashCode();
            ////System.Diagnostics.Debug.WriteLine($"hash={hash}");
            return hash;
        }
    }

    public class ImageDotKeepConfig : BindableBase
    {
        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }
    }

    public class ImageGridConfig : BindableBase
    {
    }

    public class ImageResizeFilterConfig : BindableBase
    {
    }
}