namespace NeeView
{
    public class ExportBackupCommand : CommandElement
    {
        public ExportBackupCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandExportBackup;
            this.MenuText = Properties.Resources.CommandExportBackupMenu;
            this.Note = Properties.Resources.CommandExportBackupNote;
            this.IsShowMessage = false;
        }
        
        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            SaveDataBackup.Current.ExportBackup();
        }
    }
}
