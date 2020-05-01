using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Text;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;

namespace NeeView
{
    public class BookConfig : BindableBase
    {
        private double _wideRatio = 1.0;
        private StringCollection _excludes = new StringCollection("__MACOSX;.DS_Store");
        private bool _isMultiplePageMove = true;
        private PageEndAction _pageEndAction;
        private bool _isNotifyPageLoop;
        private bool _isConfirmRecursive;
        private double _contentSpace = -1.0;
        private string _terminalSound;
        private bool _isAutoRecursive = false;
        private bool _isSortFileFirst;
        private double _bookPageSize = 300.0;
        private bool _resetPageWhenRandomSort;
        private bool _isInsertDummyPage;


        /// <summary>
        /// 横長画像判定用比率
        /// </summary>
        [PropertyMember("@ParamBookWideRatio", Tips = "@ParamBookWideRatioTips")]
        public double WideRatio
        {
            get { return _wideRatio; }
            set { SetProperty(ref _wideRatio, value); }
        }

        /// <summary>
        /// 除外フォルダー
        /// </summary>
        [PropertyMember("@ParamBookExcludes")]
        public StringCollection Excludes
        {
            get { return _excludes; }
            set { SetProperty(ref _excludes, value); }
        }

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
        public bool IsMultiplePageMove
        {
            get { return _isMultiplePageMove; }
            set { SetProperty(ref _isMultiplePageMove, value); }
        }

        // ページ終端でのアクション
        [PropertyMember("@ParamBookOperationPageEndAction")]
        public PageEndAction PageEndAction
        {
            get { return _pageEndAction; }
            set { SetProperty(ref _pageEndAction, value); }
        }

        [PropertyMember("@ParamBookOperationNotifyPageLoop")]
        public bool IsNotifyPageLoop
        {
            get { return _isNotifyPageLoop; }
            set { SetProperty(ref _isNotifyPageLoop, value); }
        }

        [PropertyPath("@ParamSeCannotMove", Filter = "Wave|*.wav")]
        public string TerminalSound
        {
            get { return _terminalSound; }
            set { SetProperty(ref  _terminalSound , string.IsNullOrWhiteSpace(value) ? null : value); }
        }

        // 再帰を確認する
        [PropertyMember("@ParamIsConfirmRecursive", Tips = "@ParamIsConfirmRecursiveTips")]
        public bool IsConfirmRecursive
        {
            get { return _isConfirmRecursive; }
            set { SetProperty(ref _isConfirmRecursive, value); }
        }

        // 自動再帰
        [PropertyMember("@ParamIsAutoRecursive")]
        public bool IsAutoRecursive
        {
            get { return _isAutoRecursive; }
            set { SetProperty(ref _isAutoRecursive, value); }
        }

        // ファイル並び順、ファイル優先
        [PropertyMember("@ParamIsSortFileFirst", Tips = "@ParamIsSortFileFirstTips")]
        public bool IsSortFileFirst
        {
            get { return _isSortFileFirst; }
            set { SetProperty(ref _isSortFileFirst, value); }
        }

        // ブックページ画像サイズ
        [PropertyRange("@ParamBookPageSize", 100.0, 600.0, TickFrequency = 10.0, IsEditable = true, Tips = "@ParamBookPageSizeTips")]
        public double BookPageSize
        {
            get { return _bookPageSize; }
            set { SetProperty(ref _bookPageSize, Math.Max(value, 64.0)); }
        }

        // ランダムソートでページをリセット
        [PropertyMember("@ParamResetPageWhenRandomSort")]
        public bool ResetPageWhenRandomSort
        {
            get { return _resetPageWhenRandomSort; }
            set { SetProperty(ref _resetPageWhenRandomSort, value); }
        }

        // ダミーページの挿入
        [PropertyMember("@ParamIsInsertDummyPage", Tips = "@ParamIsInsertDummyPageTips")]
        public bool IsInsertDummyPage
        {
            get { return _isInsertDummyPage; }
            set { SetProperty(ref _isInsertDummyPage, value); }
        }

    }
}