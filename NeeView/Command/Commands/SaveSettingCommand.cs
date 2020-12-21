namespace NeeView
{
    public class SaveSettingCommand : CommandElement
    {
        public SaveSettingCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
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
