using NeeLaboratory.ComponentModel;
using NeeView.Text;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ページの準備中に表示するもの
    /// </summary>
    [Obsolete]
    public enum LoadingPageView
    {
        [AliasName("@EnumLoadingPageViewNone")]
        None,

        [AliasName("@EnumLoadingPageViewPreThumbnail")]
        PreThumbnail,

        [AliasName("@EnumLoadingPageViewPreImage")]
        PreImage,
    }


    /// <summary>
    /// ページとするファイルの種類
    /// </summary>
    public enum BookPageCollectMode
    {
        [AliasName("@EnumBookPageCollectModeImage")]
        Image,

        [AliasName("@EnumBookPageCollectModeImageAndBook")]
        ImageAndBook,

        [AliasName("@EnumBookPageCollectModeAll")]
        All,
    }


    /// <summary>
    /// 本：設定
    /// </summary>
    public class BookProfile : BindableBase
    {
        static BookProfile() => Current = new BookProfile();
        public static BookProfile Current { get; }


        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        public bool CanPrioritizePageMove()
        {
            return Config.Current.Book.IsPrioritizePageMove && !SlideShow.Current.IsPlayingSlideShow;
        }

        /// <summary>
        /// ページ移動命令重複許可
        /// </summary>
        public bool CanMultiplePageMove()
        {
            return Config.Current.Book.IsMultiplePageMove && !SlideShow.Current.IsPlayingSlideShow;
        }

        // 除外パス判定
        public bool IsExcludedPath(string path)
        {
            return path.Split('/', '\\').Any(e => Config.Current.Book.Excludes.ConainsOrdinalIgnoreCase(e));
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember, DefaultValue(true)]
            public bool IsPrioritizePageMove { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsMultiplePageMove { get; set; }

            [DataMember, DefaultValue("__MACOSX;.DS_Store")]
            public string ExcludePath { get; set; }

            [DataMember, DefaultValue(2)]
            public int PreLoadSize { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public PreLoadMode PreLoadMode { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double WideRatio { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsEnableAnimatedGif { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsEnableNoSupportFile { get; set; }

            [DataMember, DefaultValue(BookPageCollectMode.ImageAndBook)]
            public BookPageCollectMode BookPageCollectMode { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public LoadingPageView LoadingPageView { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsLoadingPageVisible { get; set; }

            [DataMember]
            public bool IsAllFileAnImage { get; set; }

            [DataMember, DefaultValue(100)]
            public int CacheMemorySize { get; set; }

            [DataMember]
            public bool IsSortFileFirst { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    BookPageCollectMode = IsEnableNoSupportFile ? BookPageCollectMode.All : BookPageCollectMode.ImageAndBook;
                    PreLoadSize = PreLoadMode == PreLoadMode.None ? 0 : 2;
                    IsLoadingPageVisible = LoadingPageView != LoadingPageView.None;
                }
#pragma warning restore CS0612
            }

            public void RestoreConfig(Config config)
            {
                config.Performance.PreLoadSize = PreLoadSize;
                config.System.BookPageCollectMode = BookPageCollectMode;
                config.Performance.IsLoadingPageVisible = IsLoadingPageVisible;
                config.Performance.CacheMemorySize = CacheMemorySize;
                config.Image.Standard.IsAnimatedGifEnabled = IsEnableAnimatedGif;
                config.Image.Standard.IsAllFileSupported = IsAllFileAnImage;
                config.Book.WideRatio = WideRatio;
                config.Book.Excludes.OneLine = ExcludePath;
                config.Book.IsPrioritizePageMove = IsPrioritizePageMove;
                config.Book.IsMultiplePageMove = IsMultiplePageMove;
                config.Book.IsSortFileFirst = IsSortFileFirst;
            }
        }

        #endregion

    }

}
