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
        [PropertyPath("@ParamHistoryFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "XML|*.xml")]
        public string HistoryFilePath
        {
            get { return _historyFilePath; }
            set { _historyFilePath = string.IsNullOrWhiteSpace(value) || value.Trim() == SaveData.DefaultHistoryFilePath ? null : value; }
        }


        // フォルダーリストの情報記憶
        [PropertyMember("@ParamHistoryIsKeepFolderStatus")]
        public bool IsKeepFolderStatus { get; set; } = true;

        // 検索履歴の情報記憶
        [PropertyMember("@ParamHistoryIsKeepSearchHistory")]
        public bool IsKeepSearchHistory { get; set; } = true;

        /// <summary>
        /// アーカイブ内アーカイブの履歴保存
        /// </summary>
        [PropertyMember("@ParamIsInnerArchiveHistoryEnabled")]
        public bool IsInnerArchiveHistoryEnabled { get; set; } = true;

        /// <summary>
        /// UNCパスの履歴保存
        /// </summary>
        [PropertyMember("@ParamIsUncHistoryEnabled", Tips = "@ParamIsUncHistoryEnabledTips")]
        public bool IsUncHistoryEnabled { get; set; } = true;

        /// <summary>
        /// 履歴閲覧でも履歴登録日を更新する
        /// </summary>
        [PropertyMember("@ParamIsForceUpdateHistory")]
        public bool IsForceUpdateHistory { get; set; }

        /// <summary>
        /// 何回ページを切り替えたら履歴登録するか
        /// </summary>
        [PropertyMember("@ParamHistoryEntryPageCount", Tips = "@ParamHistoryEntryPageCountTips")]
        public int HistoryEntryPageCount { get; set; } = 0;

        // 履歴制限
        [PropertyMember("@ParamHistoryLimitSize")]
        public int LimitSize { get; set; } = -1;

        // 履歴制限(時間)
        [PropertyMember("@ParamHistoryLimitSpan")]
        public TimeSpan LimitSpan { get; set; }
    }
}

