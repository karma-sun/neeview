using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ExternalConfig : BindableBase
    {
        // コマンドパラメータで使用されるキーワード
        public const string KeyFile = "$File";
        public const string KeyUri = "$Uri";

        private ExternalProgramType _programType;
        private ArchiveOptionType _archiveOption = ArchiveOptionType.SendExtractFile;
        private string _archiveSeparater;


        /// <summary>
        /// ProgramType property.
        /// </summary>
        [DataMember]
        [PropertyMember("@ParamExternalProgramType")]
        public ExternalProgramType ProgramType
        {
            get { return _programType; }
            set { if (_programType != value) { _programType = value; RaisePropertyChanged(); } }
        }

        // コマンド
        [DataMember]
        [PropertyPath("@ParamExternalCommand", Tips = "@ParamExternalCommandTips", Filter = "EXE|*.exe|All|*.*")]
        public string Command { get; set; }

        // コマンドパラメータ
        // $FILE = 渡されるファイルパス
        [DataMember]
        [PropertyMember("@ParamExternalParameter")]
        public string Parameter { get; set; } = "\"" + KeyFile + "\"";

        // プロトコル
        [DataMember]
        [PropertyMember("@ParamExternalProtocol", Tips = "@ParamExternalProtocolTips")]
        public string Protocol { get; set; }

        // 複数ページのときの動作
        [DataMember]
        [PropertyMember("@ParamExternalMultiPageOption")]
        public MultiPageOptionType MultiPageOption { get; set; } = MultiPageOptionType.Once;

        // 圧縮ファイルのときの動作
        [DataMember]
        [PropertyMember("@ParamExternalArchiveOption")]
        public ArchiveOptionType ArchiveOption
        {
            get { return _archiveOption; }
            set { SetProperty(ref _archiveOption, value); }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember("@ParamExternalArchiveSeparater", EmptyMessage = "\\")]
        public string ArchiveSeparater
        {
            get => _archiveSeparater;
            set => _archiveSeparater = string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
