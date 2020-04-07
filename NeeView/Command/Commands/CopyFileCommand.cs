using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class CopyFileCommand : CommandElement
    {
        public CopyFileCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandCopyFile;
            this.MenuText = Properties.Resources.CommandCopyFileMenu;
            this.Note = Properties.Resources.CommandCopyFileNote;
            this.ShortCutKey = "Ctrl+C";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new CopyFileCommandParameter());

        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.CopyToClipboard((CopyFileCommandParameter)param);
        }
    }


    /// <summary>
    /// CopyFileCommand Parameter
    /// </summary>
    [DataContract]
    public class CopyFileCommandParameter : CommandParameter 
    {
        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private string _archiveSeparater = "\\";
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;


        // 複数ページのときの動作
        [DataMember]
        [PropertyMember("@ParamClipboardMultiPageOption")]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { SetProperty(ref _multiPagePolicy, value); }
        }

        // 圧縮ファイルのときの動作
        [DataMember]
        [PropertyMember("@ParamClipboardArchiveOption")]
        public ArchivePolicy ArchivePolicy
        {
            get { return _archivePolicy; }
            set { SetProperty(ref _archivePolicy, value); }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember("@ParamClipboardArchiveSeparater", Tips = "@ParamClipboardArchiveSeparaterTips", EmptyMessage = "\\")]
        public string ArchiveSeparater
        {
            get { return _archiveSeparater; }
            set { SetProperty(ref _archiveSeparater, string.IsNullOrEmpty(value) ? "\\" : value); }
        }


        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as CopyFileCommandParameter;
            if (target == null) return false;
            return this == target || (
                this.MultiPagePolicy == target.MultiPagePolicy &&
                this.ArchivePolicy == target.ArchivePolicy &&
                this.ArchiveSeparater == target.ArchiveSeparater);
        }
    }
}
