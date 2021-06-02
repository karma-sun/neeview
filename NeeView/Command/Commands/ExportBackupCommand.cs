using NeeView.Windows.Property;

namespace NeeView
{
    public class ExportBackupCommand : CommandElement
    {
        public ExportBackupCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ExportBackupCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ExportDataPresenter.Current.Export((ExportBackupCommandParameter)e.Parameter);
        }
    }


    public class ExportBackupCommandParameter : CommandParameter
    {
        private string _fileName;

        [PropertyPath(FileDialogType = Windows.Controls.FileDialogType.SaveFile, Filter = "NeeView BackupFile|*.nvzip")]
        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }
    }
}
