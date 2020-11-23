namespace NeeView
{
    public class ReloadSettingCommand : CommandElement
    {
        public ReloadSettingCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandReloadUserSetting;
            this.Note = Properties.Resources.CommandReloadUserSettingNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            var setting = SaveData.Current.LoadUserSetting(false);
            UserSettingTools.Restore(setting);
        }
    }
}
