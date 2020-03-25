using NeeLaboratory.ComponentModel;
using NeeView.Text;
using NeeView.Windows.Property;
using System.ComponentModel;

namespace NeeView
{
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