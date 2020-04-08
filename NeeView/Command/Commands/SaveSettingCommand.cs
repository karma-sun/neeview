namespace NeeView
{
    public class SaveSettingCommand : CommandElement
    {
        public SaveSettingCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandSaveSetting;
            this.Note = Properties.Resources.CommandSaveSettingNote;
            this.IsShowMessage = true;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            SaveDataSync.Current.SaveAll(false);
        }
    }
}
