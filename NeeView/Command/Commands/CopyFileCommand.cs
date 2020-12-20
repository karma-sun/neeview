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

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.CopyToClipboard((CopyFileCommandParameter)e.Parameter);
        }
    }


    /// <summary>
    /// CopyFileCommand Parameter
    /// </summary>
    [DataContract]
    public class CopyFileCommandParameter : CommandParameter 
    {
        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;


        // 複数ページのときの動作
        [DataMember]
        [PropertyMember]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { SetProperty(ref _multiPagePolicy, value); }
        }

        // 圧縮ファイルのときの動作
        [DataMember]
        [PropertyMember]
        public ArchivePolicy ArchivePolicy
        {
            get { return _archivePolicy; }
            set { SetProperty(ref _archivePolicy, value); }
        }
    }
}
