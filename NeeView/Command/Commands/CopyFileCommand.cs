using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class CopyFileCommand : CommandElement
    {
        public CopyFileCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
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
        private TextCopyPolicy _textCopyPolicy = TextCopyPolicy.OriginalPath;


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

        // テキストコピーのヒント
        [DataMember]
        [PropertyMember]
        public TextCopyPolicy TextCopyPolicy
        {
            get { return _textCopyPolicy; }
            set { SetProperty(ref _textCopyPolicy, value); }
        }
    }


    /// <summary>
    /// テキストコピーのヒント
    /// </summary>
    public enum TextCopyPolicy
    {
        /// <summary>
        /// テキストにしない
        /// </summary>
        None,

        /// <summary>
        /// コピーファイルの実体パス
        /// </summary>
        CopyFilePath,

        /// <summary>
        /// 元のパス
        /// </summary>
        OriginalPath,
    }
}
