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
        
        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            SaveData.Current.LoadUserSetting();
        }
    }
}
