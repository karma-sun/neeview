using NeeLaboratory.ComponentModel;
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

        #region Constructors

        private BookProfile()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        [PropertyMember("@ParamBookIsPrioritizePageMove", Tips = "@ParamBookIsPrioritizePageMoveTips")]
        public bool IsPrioritizePageMove { get; set; } = true;

        /// <summary>
        /// ページ移動命令重複許可
        /// </summary>
        [PropertyMember("@ParamBookIsMultiplePageMove", Tips = "@ParamBookIsMultiplePageMoveTips")]
        public bool IsMultiplePageMove { get; set; } = true;

        /// <summary>
        /// 先読みページ数
        /// </summary>
        [PropertyMember("@ParamPreLoadSize", Tips = "@ParamPreLoadSizeTips")]
        public int PreLoadSize { get; set; } = 2;

        /// <summary>
        /// 横長画像判定用比率
        /// </summary>
        [PropertyMember("@ParamBookWideRatio", Tips = "@ParamBookWideRatioTips")]
        public double WideRatio { get; set; } = 1.0;

        /// <summary>
        /// 除外パス
        /// </summary>
        [PropertyMember("@ParamBookExcludes", Tips = "@ParamBookExcludesTips")]
        public StringCollection Excludes { get; set; } = new StringCollection("__MACOSX;.DS_Store");

        // GIFアニメ有効
        [PropertyMember("@ParamBookIsEnableAnimatedGif", Tips = "@ParamBookIsEnableAnimatedGifTips")]
        public bool IsEnableAnimatedGif { get; set; }

        // ページ収集モード
        [PropertyMember("@ParamBookPageCollectMode", Tips = "@ParamBookPageCollectModeTips")]
        public BookPageCollectMode BookPageCollectMode { get; set; } = BookPageCollectMode.ImageAndBook;

        // ページ読み込み中表示
        [PropertyMember("@ParamBookLoadingPageView", Tips = "@ParamBookLoadingPageViewTips")]
        public LoadingPageView LoadingPageView { get; set; } = LoadingPageView.PreThumbnail;

        // サポート外ファイル有効のときに、すべてのファイルを画像とみなす
        [PropertyMember("@ParamBookIsAllFileAnImage", Tips = "@ParamBookIsAllFileAnImageTips")]
        public bool IsAllFileAnImage { get; set; }

        // キャッシュメモリサイズ (MB)
        [PropertyMember("@ParamCacheMemorySize", Tips = "@ParamCacheMemorySizeTips")]
        public int CacheMemorySize { get; set; } = 100;

        #endregion

        /// <summary>
        /// ページ移動優先設定
        /// </summary>
        public bool CanPrioritizePageMove()
        {
            return this.IsPrioritizePageMove && !SlideShow.Current.IsPlayingSlideShow;
        }

        /// <summary>
        /// ページ移動命令重複許可
        /// </summary>
        public bool CanMultiplePageMove()
        {
            return this.IsMultiplePageMove && !SlideShow.Current.IsPlayingSlideShow;
        }

        // 除外パス判定
        public bool IsExcludedPath(string path)
        {
            return path.Split('/', '\\').Any(e => this.Excludes.Contains(e));
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

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

            [DataMember, DefaultValue(false)]
            public bool IsEnableAnimatedGif { get; set; }

            [Obsolete, DataMember]
            public bool IsEnableNoSupportFile { get; set; }

            [DataMember, DefaultValue(BookPageCollectMode.ImageAndBook)]
            public BookPageCollectMode BookPageCollectMode { get; set; }

            [DataMember, DefaultValue(LoadingPageView.PreThumbnail)]
            public LoadingPageView LoadingPageView { get; set; }

            [DataMember]
            public bool IsAllFileAnImage { get; set; }

            [DataMember, DefaultValue(100)]
            public int CacheMemorySize { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                if (_Version < Config.GenerateProductVersionNumber(34, 0, 0))
                {
                    BookPageCollectMode = IsEnableNoSupportFile ? BookPageCollectMode.All : BookPageCollectMode.ImageAndBook;
                    PreLoadSize = PreLoadMode == PreLoadMode.None ? 0 : 2;
                }
#pragma warning restore CS0612
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsPrioritizePageMove = this.IsPrioritizePageMove;
            memento.IsMultiplePageMove = this.IsMultiplePageMove;
            memento.PreLoadSize = this.PreLoadSize;
            memento.WideRatio = this.WideRatio;
            memento.ExcludePath = this.Excludes.ToString();
            memento.IsEnableAnimatedGif = this.IsEnableAnimatedGif;
            memento.BookPageCollectMode = this.BookPageCollectMode;
            memento.LoadingPageView = this.LoadingPageView;
            memento.IsAllFileAnImage = this.IsAllFileAnImage;
            memento.CacheMemorySize = this.CacheMemorySize;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsPrioritizePageMove = memento.IsPrioritizePageMove;
            this.IsMultiplePageMove = memento.IsMultiplePageMove;
            this.PreLoadSize = memento.PreLoadSize;
            this.WideRatio = memento.WideRatio;
            this.Excludes.FromString(memento.ExcludePath);
            this.IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
            this.BookPageCollectMode = memento.BookPageCollectMode;
            this.LoadingPageView = memento.LoadingPageView;
            this.IsAllFileAnImage = memento.IsAllFileAnImage;
            this.CacheMemorySize = memento.CacheMemorySize;
        }
        #endregion

    }

}
