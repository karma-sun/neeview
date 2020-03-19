using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    public class HistoryConfig : BindableBase
    {
        private bool _isSaveHistory = true;
        private string _historyFilePath;

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
    }
}

