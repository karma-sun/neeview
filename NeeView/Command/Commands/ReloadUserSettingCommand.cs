namespace NeeView
{
    public class ReloadUserSettingCommand : CommandElement
    {
        public ReloadUserSettingCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandReloadUserSetting;
            this.Note = Properties.Resources.CommandReloadUserSettingNote;
            this.IsShowMessage = false;
        }
        
        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SaveData.Current.LoadUserSetting();
        }
    }
}
