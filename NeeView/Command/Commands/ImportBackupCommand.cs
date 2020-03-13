namespace NeeView
{
    public class ImportBackupCommand : CommandElement
    {
        public ImportBackupCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandImportBackup;
            this.MenuText = Properties.Resources.CommandImportBackupMenu;
            this.Note = Properties.Resources.CommandImportBackupNote;
            this.IsShowMessage = false;
        }
        
        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            SaveDataBackup.Current.ImportBackup();
        }
    }
}
