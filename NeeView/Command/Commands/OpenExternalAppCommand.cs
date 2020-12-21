using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class OpenExternalAppCommand : CommandElement
    {
        public OpenExternalAppCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new OpenExternalAppCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.OpenApplication((OpenExternalAppCommandParameter)e.Parameter);
        }
    }

    [DataContract]
    public class OpenExternalAppCommandParameter : CommandParameter, INotifyPropertyChanged
    {
        // コマンドパラメータで使用されるキーワード
        public const string KeyFile = "$File";
        public const string KeyUri = "$Uri";
        public const string DefaultParameter = "\"" + KeyFile + "\"";

        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private string _command;
        private string _parameter = DefaultParameter;
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;


        // コマンド
        [DataMember]
        [PropertyPath(Filter = "EXE|*.exe|All|*.*")]
        public string Command
        {
            get { return _command; }
            set { SetProperty(ref _command, value); }
        }

        // コマンドパラメータ
        // $FILE = 渡されるファイルパス
        [DataMember]
        [PropertyMember]
        public string Parameter
        {
            get { return _parameter; }
            set { SetProperty(ref _parameter, string.IsNullOrWhiteSpace(value) ? DefaultParameter : value); }
        }

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
