namespace NeeView
{
    public class ExportBackupCommand : CommandElement
    {
        public ExportBackupCommand() : base(CommandType.ExportBackup)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandExportBackup;
            this.MenuText = Properties.Resources.CommandExportBackupMenu;
            this.Note = Properties.Resources.CommandExportBackupNote;
            this.IsShowMessage = false;
        }
        
        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SaveDataBackup.Current.ExportBackup();
        }
    }
}
