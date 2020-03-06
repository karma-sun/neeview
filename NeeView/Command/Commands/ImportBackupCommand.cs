namespace NeeView
{
    public class ImportBackupCommand : CommandElement
    {
        public ImportBackupCommand() : base(CommandType.ImportBackup)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandImportBackup;
            this.MenuText = Properties.Resources.CommandImportBackupMenu;
            this.Note = Properties.Resources.CommandImportBackupNote;
            this.IsShowMessage = false;
        }
        
        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SaveDataBackup.Current.ImportBackup();
        }
    }
}
