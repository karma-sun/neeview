using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class CopyImageCommand : CommandElement
    {
        public CopyImageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandCopyImage;
            this.MenuText = Properties.Resources.CommandCopyImageMenu;
            this.Note = Properties.Resources.CommandCopyImageNote;
            this.ShortCutKey = "Ctrl+Shift+C";
            this.IsShowMessage = true;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return ContentCanvas.Current.CanCopyImageToClipboard();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvas.Current.CopyImageToClipboard();
        }
    }


    /// <summary>
    /// CopyFileCommand Parameter
    /// </summary>
    [DataContract]
    public class CopyFileCommandParameter : CommandParameter, INotifyPropertyChanged
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
