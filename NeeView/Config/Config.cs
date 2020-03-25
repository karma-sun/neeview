using NeeLaboratory.ComponentModel;
using NeeView.Text;
using NeeView.Windows.Property;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

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

        public ImageGridConfig ImageGrid { get; set; } = new ImageGridConfig();

        public ImageResizeFilterConfig ImageResizeFilter { get; set; } = new ImageResizeFilterConfig();

        public ViewConfig View { get; set; } = new ViewConfig();

        public MouseConfig Mouse { get; set; } = new MouseConfig();

        public TouchConfig Touch { get; set; } = new TouchConfig();

        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        public BookConfig Book { get; set; } = new BookConfig();

        public CommandConfig Command { get; set; } = new CommandConfig();

        public ScriptConfig Script { get; set; } = new ScriptConfig();
    }


    public class BookConfig : BindableBase
    {
        private double _contentSpace = -1.0;
        private string _terminalSound;
        private bool _isAutoRecursive = false;


        /// <summary>
        /// 横長画像判定用比率
        /// </summary>
        [PropertyMember("@ParamBookWideRatio", Tips = "@ParamBookWideRatioTips")]
        public double WideRatio { get; set; } = 1.0;

        /// <summary>
        /// 除外フォルダー
        /// </summary>
        [PropertyMember("@ParamBookExcludes")]
        public StringCollection Excludes { get; set; } = new StringCollection("__MACOSX;.DS_Store");

        // 2ページコンテンツの隙間
        [DefaultValue(-1.0)]
        [PropertyRange("@ParamContentCanvasContentsSpace", -32, 32, TickFrequency = 1, Tips = "@ParamContentCanvasContentsSpaceTips")]
        public double ContentsSpace
        {
            get { return _contentSpace; }
            set { SetProperty(ref _contentSpace, value); }
        }

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
        
        // ページ終端でのアクション
        [PropertyMember("@ParamBookOperationPageEndAction")]
        public PageEndAction PageEndAction { get; set; }

        [PropertyMember("@ParamBookOperationNotifyPageLoop")]
        public bool IsNotifyPageLoop { get; set; }

        [PropertyPath("@ParamSeCannotMove", Filter = "Wave|*.wav")]
        public string TerminalSound
        {
            get { return _terminalSound; }
            set { _terminalSound = string.IsNullOrWhiteSpace(value) ? null : value; }
        }

        // 再帰を確認する
        [PropertyMember("@ParamIsConfirmRecursive", Tips = "@ParamIsConfirmRecursiveTips")]
        public bool IsConfirmRecursive { get; set; }

        // 自動再帰
        [PropertyMember("@ParamIsAutoRecursive")]
        public bool IsAutoRecursive
        {
            get { return _isAutoRecursive; }
            set { SetProperty(ref _isAutoRecursive, value); }
        }

        // ファイル並び順、ファイル優先
        [PropertyMember("@ParamIsSortFileFirst", Tips = "@ParamIsSortFileFirstTips")]
        public bool IsSortFileFirst { get; set; }
    }
}