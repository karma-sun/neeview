using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ClipboardConfig : BindableBase
    {
        private ArchiveOptionType _archiveOption = ArchiveOptionType.SendExtractFile;
        private string _archiveSeparater;


        // 複数ページのときの動作
        [DataMember]
        [PropertyMember("@ParamClipboardMultiPageOption")]
        public MultiPageOptionType MultiPageOption { get; set; } = MultiPageOptionType.Once;

        // 圧縮ファイルのときの動作
        [DataMember]
        [PropertyMember("@ParamClipboardArchiveOption")]
        public ArchiveOptionType ArchiveOption
        {
            get { return _archiveOption; }
            set { SetProperty(ref _archiveOption, value); }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember("@ParamClipboardArchiveSeparater", EmptyMessage = "\\")]
        public string ArchiveSeparater
        {
            get => _archiveSeparater;
            set => _archiveSeparater = string.IsNullOrEmpty(value) ? null : value;
        }
    }
}

