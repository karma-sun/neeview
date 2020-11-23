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

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            SaveDataSync.Current.SaveAll(false);
        }
    }
}
