using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ClipboardConfig : BindableBase
    {
        private ArchiveOptionType _archiveOption = ArchiveOptionType.SendExtractFile;
        private string _archiveSeparater;
        private MultiPageOptionType _multiPageOption = MultiPageOptionType.Once;


        // 複数ページのときの動作
        [DataMember]
        [PropertyMember("@ParamClipboardMultiPageOption")]
        public MultiPageOptionType MultiPageOption
        {
            get { return _multiPageOption; }
            set { SetProperty(ref _multiPageOption, value); }
        }

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
            get { return _archiveSeparater; }
            set { SetProperty(ref _archiveSeparater, string.IsNullOrEmpty(value) ? null : value); }
        }
    }
}

