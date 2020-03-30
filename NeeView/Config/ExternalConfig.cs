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
        private string _command;
        private string _parameter = "\"" + KeyFile + "\"";
        private string _protocol;
        private MultiPageOptionType _multiPageOption = MultiPageOptionType.Once;


        /// <summary>
        /// ProgramType property.
        /// </summary>
        [DataMember]
        [PropertyMember("@ParamExternalProgramType")]
        public ExternalProgramType ProgramType
        {
            get { return _programType; }
            set { SetProperty(ref _programType, value); }
        }

        // コマンド
        [DataMember]
        [PropertyPath("@ParamExternalCommand", Tips = "@ParamExternalCommandTips", Filter = "EXE|*.exe|All|*.*")]
        public string Command
        {
            get { return _command; }
            set { SetProperty(ref _command, value); }
        }

        // コマンドパラメータ
        // $FILE = 渡されるファイルパス
        [DataMember]
        [PropertyMember("@ParamExternalParameter")]
        public string Parameter
        {
            get { return _parameter; }
            set { SetProperty(ref _parameter, value); }
        }

        // プロトコル
        [DataMember]
        [PropertyMember("@ParamExternalProtocol", Tips = "@ParamExternalProtocolTips")]
        public string Protocol
        {
            get { return _protocol; }
            set { SetProperty(ref _protocol, value); }
        }

        // 複数ページのときの動作
        [DataMember]
        [PropertyMember("@ParamExternalMultiPageOption")]
        public MultiPageOptionType MultiPageOption
        {
            get { return _multiPageOption; }
            set { SetProperty(ref _multiPageOption, value); }
        }

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
            get { return _archiveSeparater; }
            set { SetProperty(ref _archiveSeparater, string.IsNullOrEmpty(value) ? null : value); }
        }
    }
}
