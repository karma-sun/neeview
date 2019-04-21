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

        private int _cacheMemorySize = 100;
        private int _maxCacheMemorySize;

        #region Constructors

        private BookProfile()
        {
            _maxCacheMemorySize = GetMaxCacheMemorySize();
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
        public bool IsEnableAnimatedGif { get; set; } = true;

        // ページ収集モード
        [PropertyMember("@ParamBookPageCollectMode", Tips = "@ParamBookPageCollectModeTips")]
        public BookPageCollectMode BookPageCollectMode { get; set; } = BookPageCollectMode.ImageAndBook;

        // ページ読み込み中表示
        [PropertyMember("@ParamBookIsLoadingPageVisible", Tips = "@ParamBookIsLoadingPageVisibleTips")]
        public bool IsLoadingPageVisible { get; set; } = true;

        // サポート外ファイル有効のときに、すべてのファイルを画像とみなす
        [PropertyMember("@ParamBookIsAllFileAnImage", Tips = "@ParamBookIsAllFileAnImageTips")]
        public bool IsAllFileAnImage { get; set; }

        // キャッシュメモリサイズ (MB)
        [PropertyMember("@ParamCacheMemorySize", Tips = "@ParamCacheMemorySizeTips")]
        public int CacheMemorySize
        {
            // 64bit,32bit共用のため、設定時、取得時に最大メモリ制限をしている
            get { return Math.Min(_cacheMemorySize, _maxCacheMemorySize); }
            set { SetProperty(ref _cacheMemorySize, Math.Min(value, _maxCacheMemorySize)); }
        }

        #endregion

        /// <summary>
        /// 最大キャッシュメモリサイズ計算
        /// </summary>
        private int GetMaxCacheMemorySize()
        {
            int max = (int)(Config.GetTotalPhysicalMemory() / 1024 / 1024);

            // -2GB or half size
            max = Math.Max(max - 2 * 1024, max / 2);

            // if 32bit, limit 2GB
            if (!Config.IsX64)
            {
                max = Math.Min(max, 2 * 1024);
            }

            return max;
        }

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
                    IsLoadingPageVisible = LoadingPageView != LoadingPageView.None;
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
            memento.ExcludePath = this.Excludes.OneLine;
            memento.IsEnableAnimatedGif = this.IsEnableAnimatedGif;
            memento.BookPageCollectMode = this.BookPageCollectMode;
            memento.IsLoadingPageVisible = this.IsLoadingPageVisible;
            memento.IsAllFileAnImage = this.IsAllFileAnImage;
            memento.CacheMemorySize = _cacheMemorySize;
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
            this.Excludes.OneLine = memento.ExcludePath;
            this.IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
            this.BookPageCollectMode = memento.BookPageCollectMode;
            this.IsLoadingPageVisible = memento.IsLoadingPageVisible;
            this.IsAllFileAnImage = memento.IsAllFileAnImage;
            _cacheMemorySize = memento.CacheMemorySize;
        }
        #endregion

    }

}
