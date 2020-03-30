using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    public class HistoryConfig : BindableBase
    {
        private PanelListItemStyle _panelListItemStyle;
        private bool _isSaveHistory = true;
        private string _historyFilePath;
        private bool _isKeepFolderStatus = true;
        private bool _isKeepSearchHistory = true;
        private bool _isInnerArchiveHistoryEnabled = true;
        private bool _isUncHistoryEnabled = true;
        private bool _isForceUpdateHistory;
        private int _historyEntryPageCount = 0;
        private int _limitSize = -1;
        private TimeSpan _limitSpan;

        [PropertyMember("@ParamHistoryListItemStyle")]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        // 履歴データの保存
        [PropertyMember("@ParamIsSaveHistory")]
        public bool IsSaveHistory
        {
            get { return _isSaveHistory; }
            set { SetProperty(ref _isSaveHistory, value); }
        }

        // 履歴データの保存場所
        [PropertyPath("@ParamHistoryFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "JSON|*.json")]
        public string HistoryFilePath
        {
            get { return _historyFilePath; }
            set { SetProperty(ref _historyFilePath, (string.IsNullOrWhiteSpace(value) || value.Trim() == SaveData.DefaultHistoryFilePath) ? null : value); }
        }

        // フォルダーリストの情報記憶
        [PropertyMember("@ParamHistoryIsKeepFolderStatus")]
        public bool IsKeepFolderStatus
        {
            get { return _isKeepFolderStatus; }
            set { SetProperty(ref _isKeepFolderStatus, value); }
        }

        // 検索履歴の情報記憶
        [PropertyMember("@ParamHistoryIsKeepSearchHistory")]
        public bool IsKeepSearchHistory
        {
            get { return _isKeepSearchHistory; }
            set { SetProperty(ref _isKeepSearchHistory, value); }
        }

        /// <summary>
        /// アーカイブ内アーカイブの履歴保存
        /// </summary>
        [PropertyMember("@ParamIsInnerArchiveHistoryEnabled")]
        public bool IsInnerArchiveHistoryEnabled
        {
            get { return _isInnerArchiveHistoryEnabled; }
            set { SetProperty(ref _isInnerArchiveHistoryEnabled, value); }
        }

        /// <summary>
        /// UNCパスの履歴保存
        /// </summary>
        [PropertyMember("@ParamIsUncHistoryEnabled", Tips = "@ParamIsUncHistoryEnabledTips")]
        public bool IsUncHistoryEnabled
        {
            get { return _isUncHistoryEnabled; }
            set { SetProperty(ref _isUncHistoryEnabled, value); }
        }

        /// <summary>
        /// 履歴閲覧でも履歴登録日を更新する
        /// </summary>
        [PropertyMember("@ParamIsForceUpdateHistory")]
        public bool IsForceUpdateHistory
        {
            get { return _isForceUpdateHistory; }
            set { SetProperty(ref _isForceUpdateHistory, value); }
        }

        /// <summary>
        /// 何回ページを切り替えたら履歴登録するか
        /// </summary>
        [PropertyMember("@ParamHistoryEntryPageCount", Tips = "@ParamHistoryEntryPageCountTips")]
        public int HistoryEntryPageCount
        {
            get { return _historyEntryPageCount; }
            set { SetProperty(ref _historyEntryPageCount, value); }
        }

        // 履歴制限
        [PropertyMember("@ParamHistoryLimitSize")]
        public int LimitSize
        {
            get { return _limitSize; }
            set { SetProperty(ref _limitSize, value); }
        }

        // 履歴制限(時間)
        [PropertyMember("@ParamHistoryLimitSpan")]
        public TimeSpan LimitSpan
        {
            get { return _limitSpan; }
            set { SetProperty(ref _limitSpan, value); }
        }

    }
}

