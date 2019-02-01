using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
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
    /// 本：設定
    /// </summary>
    public class BookProfile : BindableBase
    {
        static BookProfile() => Current = new BookProfile();
        public static BookProfile Current { get; }

        private bool _isEnableNoSupportFile;


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
        /// 先読みモード
        /// </summary>
        [PropertyMember("@ParamBookPreLoadMode", Tips = "@ParamBookPreLoadModeTips")]
        public PreLoadMode PreLoadMode { get; set; } = PreLoadMode.PreLoad;

        /// <summary>
        /// 先読み自動判定許サイズ
        /// </summary>
        [PropertyMember("@ParamBookPreloadLimitSize", Tips = "@ParamBookPreloadLimitSizeTips")]
        public Size PreloadLimitSize { get; set; } = new Size(4096, 4096);

        /// <summary>
        /// 先読み自動判定許サイズ
        /// </summary>
        public int PreLoadLimitSize
        {
            get { return (int)(PreloadLimitSize.Width * PreloadLimitSize.Height); }
        }

        /// <summary>
        /// WideRatio property.
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

        // サポート外ファイル有効
        [PropertyMember("@ParamBookIsEnableNoSupportFile", Tips = "@ParamBookIsEnableNoSupportFileTips")]
        public bool IsEnableNoSupportFile
        {
            get { return _isEnableNoSupportFile; }
            set { SetProperty(ref _isEnableNoSupportFile, value); }
        }

        // ページ読み込み中表示
        [PropertyMember("@ParamBookLoadingPageView", Tips = "@ParamBookLoadingPageViewTips")]
        public LoadingPageView LoadingPageView { get; set; } = LoadingPageView.PreThumbnail;

        // サポート外ファイル有効のときに、すべてのファイルを画像とみなす
        [PropertyMember("@ParamBookIsAllFileAnImage")]
        public bool IsAllFileAnImage { get; set; }

        #endregion

        /// <summary>
        /// 画像ファイルの拡張子判定無効
        /// </summary>
        public bool IsIgnoreFileExtension()
        {
            return IsEnableNoSupportFile && IsAllFileAnImage;
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
            [DataMember, DefaultValue(true)]
            public bool IsPrioritizePageMove { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsMultiplePageMove { get; set; }

            [DataMember, DefaultValue("__MACOSX;.DS_Store")]
            public string ExcludePath { get; set; }

            [DataMember, DefaultValue(PreLoadMode.PreLoad)]
            public PreLoadMode PreLoadMode { get; set; }

            [DataMember, DefaultValue(typeof(Size), "4096,4096")]
            public Size PreloadLimitSize { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double WideRatio { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsEnableAnimatedGif { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsEnableNoSupportFile { get; set; }

            [DataMember, DefaultValue(LoadingPageView.PreThumbnail)]
            public LoadingPageView LoadingPageView { get; set; }

            [DataMember]
            public bool IsAllFileAnImage { get; set; }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsPrioritizePageMove = this.IsPrioritizePageMove;
            memento.IsMultiplePageMove = this.IsMultiplePageMove;
            memento.PreLoadMode = this.PreLoadMode;
            memento.PreloadLimitSize = this.PreloadLimitSize;
            memento.WideRatio = this.WideRatio;
            memento.ExcludePath = this.Excludes.ToString();
            memento.IsEnableAnimatedGif = this.IsEnableAnimatedGif;
            memento.IsEnableNoSupportFile = this.IsEnableNoSupportFile;
            memento.LoadingPageView = this.LoadingPageView;
            memento.IsAllFileAnImage = this.IsAllFileAnImage;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsPrioritizePageMove = memento.IsPrioritizePageMove;
            this.IsMultiplePageMove = memento.IsMultiplePageMove;
            this.PreLoadMode = memento.PreLoadMode;
            this.PreloadLimitSize = memento.PreloadLimitSize;
            this.WideRatio = memento.WideRatio;
            this.Excludes.FromString(memento.ExcludePath);
            this.IsEnableAnimatedGif = memento.IsEnableAnimatedGif;
            this.IsEnableNoSupportFile = memento.IsEnableNoSupportFile;
            this.LoadingPageView = memento.LoadingPageView;
            this.IsAllFileAnImage = memento.IsAllFileAnImage;
        }
        #endregion

    }

}
