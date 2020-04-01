using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class OpenExternalAppCommand : CommandElement
    {
        public OpenExternalAppCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandOpenApplication;
            this.Note = Properties.Resources.CommandOpenApplicationNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new OpenExternalAppCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.OpenApplication((OpenExternalAppCommandParameter)param);
        }
    }

    [DataContract]
    public class OpenExternalAppCommandParameter : CommandParameter, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        // コマンドパラメータで使用されるキーワード
        public const string KeyFile = "$File";
        public const string KeyUri = "$Uri";
        public const string DefaultParameter = "\"" + KeyFile + "\"";

        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private string _archiveSeparater = "\\";
        private string _command;
        private string _parameter = DefaultParameter;
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;


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
        [PropertyMember("@ParamExternalParameter", Tips = "@ParamExternalParameterTips")]
        public string Parameter
        {
            get { return _parameter; }
            set { SetProperty(ref _parameter, string.IsNullOrWhiteSpace(value) ? DefaultParameter : value); }
        }

        // 複数ページのときの動作
        [DataMember]
        [PropertyMember("@ParamExternalMultiPageOption")]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { SetProperty(ref _multiPagePolicy, value); }
        }

        // 圧縮ファイルのときの動作
        [DataMember]
        [PropertyMember("@ParamExternalArchiveOption")]
        public ArchivePolicy ArchivePolicy
        {
            get { return _archivePolicy; }
            set { SetProperty(ref _archivePolicy, value); }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember("@ParamExternalArchiveSeparater", Tips = "@ParamExternalArchiveSeparaterTips", EmptyMessage = "\\")]
        public string ArchiveSeparater
        {
            get { return _archiveSeparater; }
            set { SetProperty(ref _archiveSeparater, string.IsNullOrEmpty(value) ? "\\" : value); }
        }


        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as OpenExternalAppCommandParameter;
            if (target == null) return false;
            return this == target || (this.Command == target.Command &&
                this.Parameter == target.Parameter &&
                this.MultiPagePolicy == target.MultiPagePolicy &&
                this.ArchivePolicy == target.ArchivePolicy &&
                this.ArchiveSeparater == target.ArchiveSeparater);
        }
    }

}
